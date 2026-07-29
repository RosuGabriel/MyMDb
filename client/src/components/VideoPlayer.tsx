import React, { forwardRef, useEffect, useState, useCallback } from "react";
import { Attribute, staticClient, apiClient, API_URL } from "../Data";

interface VideoPlayerProps {
  src: string; // This is now the media ID (GUID)
  attributes?: Attribute[];
  className?: string;
}

interface SubtitleTrack {
  language: string;
  blobUrl: string;
}

const VideoPlayer = forwardRef<HTMLVideoElement, VideoPlayerProps>(
  ({ src, attributes, className }, ref) => {
    const [streamUrl, setStreamUrl] = useState<string | null>(null);
    const [subtitleTracks, setSubtitleTracks] = useState<SubtitleTrack[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const initializeStream = useCallback(async () => {
      if (!src) return;

      setLoading(true);
      setError(null);

      try {
        // First, get the streaming token cookie via authenticated request
        await apiClient.post(`media/stream-token/${src}`);

        // Now set the stream URL - browser will send the cookie automatically
        const url = `${API_URL}api/media/stream/${src}`;
        setStreamUrl(url);
        setLoading(false);
      } catch (err: any) {
        console.error("Failed to initialize stream:", err);
        const message = err.response?.data || "Failed to load video";
        setError(typeof message === "string" ? message : "Failed to load video");
        setLoading(false);
      }
    }, [src]);

    const fetchSubtitles = useCallback(async () => {
      if (!attributes) return;

      const subtitles = attributes.filter(
        (attr) => attr.type.toLowerCase() === "subtitle",
      );

      const tracks: SubtitleTrack[] = [];

      for (const subtitle of subtitles) {
        try {
          const response = await staticClient.get(subtitle.attributePath, {
            responseType: "blob",
          });
          const url = URL.createObjectURL(response.data);
          tracks.push({ language: subtitle.language, blobUrl: url });
        } catch (err) {
          console.error(
            `Failed to fetch subtitle for ${subtitle.language}:`,
            err,
          );
        }
      }

      setSubtitleTracks(tracks);
    }, [attributes]);

    useEffect(() => {
      initializeStream();
      fetchSubtitles();

      return () => {
        // Clean up subtitle blob URLs on unmount
        subtitleTracks.forEach((track) => URL.revokeObjectURL(track.blobUrl));
      };
    }, [src]);

    if (error) {
      return <div className="text-danger">{error}</div>;
    }

    return (
      <>
        {loading && <div className="text-center p-4">Loading video...</div>}
        <video
          ref={ref}
          src={streamUrl || undefined}
          controls
          crossOrigin="use-credentials"
          className={className || "video-fluid w-100"}
          style={{ display: loading ? "none" : "block" }}
        >
          {subtitleTracks.map((track) => (
            <track
              key={track.language}
              kind="subtitles"
              src={track.blobUrl}
              srcLang={track.language}
              label={track.language}
            />
          ))}
          Your browser does not support the video tag.
        </video>
      </>
    );
  },
);

export default VideoPlayer;
