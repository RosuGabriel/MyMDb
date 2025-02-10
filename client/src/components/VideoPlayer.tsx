import React, { forwardRef, useEffect, useState } from "react";
import { Attribute, staticClient } from "../Data";
interface VideoPlayerProps {
  src: string;
  attributes?: Attribute[];
}

const VideoPlayer = forwardRef<HTMLVideoElement, VideoPlayerProps>(
  ({ src, attributes }, ref) => {
    const [loading, setLoading] = useState<boolean>(true);
    const [rangeStart, setRangeStart] = useState<number>(0);
    const CHUNK_SIZE = 10 * 1024 * 1024;
    const [mediaSource, setMediaSource] = useState<MediaSource | null>(null);
    const [sourceBuffer, setSourceBuffer] = useState<SourceBuffer | null>(null);
    const [videoElement, setVideoElement] = useState<HTMLVideoElement | null>(
      null
    );
    const [finished, setFinished] = useState<boolean>(false);

    useEffect(() => {
      if (!("MediaSource" in window)) {
        console.error("MediaSource is not supported in this browser.");
        return;
      }
      const mediaSourceInstance = new MediaSource();
      setMediaSource(mediaSourceInstance);
    }, []);

    const fetchVideo = async (retryCount = 3) => {
      try {
        const response = await staticClient.get(src, {
          responseType: "blob",
          headers: {
            Range: `bytes=${rangeStart}-${rangeStart + CHUNK_SIZE - 1}`,
          },
        });

        if (response.headers["content-range"]) {
          const contentRange = response.headers["content-range"];
          const totalSize = parseInt(contentRange.split("/")[1]);

          if (rangeStart + CHUNK_SIZE >= totalSize) {
            setFinished(true);
          }
        }

        const videoBlob = response.data;

        if (mediaSource && sourceBuffer && !sourceBuffer.updating) {
          sourceBuffer.appendBuffer(await videoBlob.arrayBuffer());
        }

        setRangeStart(rangeStart + CHUNK_SIZE);
      } catch (error) {
        if (retryCount > 0) {
          await fetchVideo(retryCount - 1);
        } else {
          console.error("Failed to fetch video:", error);
        }
      }
    };

    useEffect(() => {
      if (src && sourceBuffer && !finished) {
        fetchVideo();
      }
    }, [src, rangeStart, sourceBuffer, finished]);

    useEffect(() => {
      if (ref && typeof ref !== "function") {
        setVideoElement(ref.current);
      }
    }, [ref]);

    useEffect(() => {
      if (videoElement && mediaSource) {
        videoElement.src = URL.createObjectURL(mediaSource);

        mediaSource.addEventListener("sourceopen", () => {
          try {
            const buffer = mediaSource.addSourceBuffer(
              'video/mp4; codecs="hev1.1.6.L93.B0"'
            );
            setSourceBuffer(buffer);
          } catch (error) {
            console.error("Failed to add source buffer:", error);
          }
        });

        mediaSource.addEventListener("sourceended", () =>
          console.log("MediaSource stream ended")
        );
      }
    }, [videoElement, mediaSource]);

    useEffect(() => {
      if (finished && mediaSource && mediaSource.readyState === "open") {
        try {
          mediaSource.endOfStream();
        } catch (err) {
          console.error("Error ending MediaSource stream:", err);
        }
      }
    }, [finished, mediaSource]);

    return (
      <video ref={ref} controls className="video-fluid w-100" preload="none">
        {loading && <p>Video loading...</p>}
        {!loading && finished && <p>Video ready</p>}
        {attributes &&
          attributes.map((attribute) => {
            if (attribute.type === "Subtitle") {
              return (
                <track
                  key={attribute.language}
                  kind="subtitles"
                  src={`static/${attribute.attributePath}`}
                  srcLang={attribute.language}
                  label={attribute.language}
                />
              );
            }
            return null;
          })}
        Your browser does not support the video tag.
      </video>
    );
  }
);

export default VideoPlayer;
