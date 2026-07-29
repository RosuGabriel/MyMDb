import React, { useEffect, useState } from "react";
import { editEpisode, fetchMediaById } from "../services/MediaService";
import { useNavigate, useParams } from "react-router-dom";
import { Media } from "../Data";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const EditEpisode: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [releaseDate, setReleaseDate] = useState("");
  const [seasonNumber, setSeason] = useState(1);
  const [episodeNumber, setEpisodeNumber] = useState(1);
  const [seriesId, setSeriesId] = useState("");
  const [posterPath, setPosterPath] = useState("");
  const [videoPath, setVideoPath] = useState("");
  const [image, setImage] = useState<File | null>(null);
  const [video, setVideo] = useState<File | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchEpisode = async () => {
      if (id) {
        try {
          const episode = await fetchMediaById(id);
          setTitle(episode.title || "");
          setDescription(episode.description || "");
          setPosterPath(episode.posterPath || "");
          setVideoPath(episode.videoPath || "");
          if (episode.releaseDate) {
            const date = new Date(episode.releaseDate);
            setReleaseDate(date.toISOString().split("T")[0]);
          }
          setSeason(episode.seasonNumber || 1);
          setEpisodeNumber(episode.episodeNumber || 1);
          setSeriesId(episode.seriesId || "");
        } catch (error) {
          console.error("Error fetching episode:", error);
        } finally {
          setIsLoading(false);
        }
      }
    };

    fetchEpisode();
  }, [id]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    setIsEditing(true);

    const editedEpisode: Partial<Media> = {
      title,
      description,
      posterPath,
      videoPath,
      releaseDate: releaseDate ? new Date(releaseDate) : undefined,
      seasonNumber,
      episodeNumber,
      seriesId: seriesId as any,
    };

    const handleUploadProgress = (progressEvent: ProgressEvent) => {
      const progress = Math.round(
        (progressEvent.loaded / progressEvent.total) * 100,
      );
      setUploadProgress(progress);
      if (progressEvent.loaded === progressEvent.total) {
        console.log("Upload complete!");
        setUploadProgress(100);
      }
    };

    try {
      console.log("Updating episode...");
      await editEpisode(id, editedEpisode, image, video, handleUploadProgress);
      console.log("Episode updated successfully");
    } catch (error) {
      console.error("Error editing episode:", error);
      setIsEditing(false);
    }
  };

  useEffect(() => {
    if (uploadProgress === 100) {
      setUploadProgress(0);
      navigate("/mymdb/media/" + seriesId);
    }
  }, [uploadProgress, seriesId, navigate]);

  if (isLoading) {
    return <h1>Loading...</h1>;
  }

  return (
    <form className="create-form" onSubmit={handleSubmit}>
      <h2>Edit Episode</h2>
      <div className="mb-3">
        <label htmlFor="title" className="form-label">
          Title
        </label>
        <input
          className="form-control"
          type="text"
          id="title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
      </div>
      <div className="mb-3">
        <label htmlFor="season" className="form-label">
          Season
        </label>
        <input
          className="form-control"
          type="number"
          id="season"
          value={seasonNumber}
          onChange={(e) => setSeason(parseInt(e.target.value))}
        />
      </div>
      <div className="mb-3">
        <label htmlFor="episode-number" className="form-label">
          Episode Number
        </label>
        <input
          className="form-control"
          type="number"
          id="episode-number"
          value={episodeNumber}
          onChange={(e) => setEpisodeNumber(parseInt(e.target.value))}
        />
      </div>
      <div className="mb-3">
        <label htmlFor="description" className="form-label">
          Description
        </label>
        <textarea
          className="form-control"
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
      </div>
      <div className="mb-3">
        <label htmlFor="releaseDate" className="form-label">
          Release Date
        </label>
        <input
          className="form-control"
          type="date"
          id="releaseDate"
          value={releaseDate}
          onChange={(e) => setReleaseDate(e.target.value)}
        />
      </div>
      <div className="mb-3">
        <label htmlFor="posterPath" className="form-label">
          Poster Path
        </label>
        <input
          className="form-control"
          type="text"
          id="posterPath"
          value={posterPath}
          onChange={(e) => setPosterPath(e.target.value)}
          placeholder="e.g., media/images/poster.png"
        />
      </div>
      <div className="mb-3">
        <label htmlFor="image" className="form-label">
          Poster File (Optional)
        </label>
        <input
          className="form-control"
          type="file"
          id="image"
          accept="image/*"
          onChange={(e) => setImage(e.target.files?.[0] || null)}
        />
      </div>
      <div className="mb-3">
        <label htmlFor="videoPath" className="form-label">
          Video Path
        </label>
        <input
          className="form-control"
          type="text"
          id="videoPath"
          value={videoPath}
          onChange={(e) => setVideoPath(e.target.value)}
          placeholder="e.g., media/videos/episode.mp4"
        />
      </div>
      <div className="mb-3">
        <label htmlFor="video" className="form-label">
          Video File (Optional)
        </label>
        <input
          className="form-control"
          type="file"
          id="video"
          accept="video/*"
          onChange={(e) => setVideo(e.target.files?.[0] || null)}
        />
      </div>

      <div className="d-flex flex-row justify-content-center align-items-center mt-4 gap-2">
        {isEditing ? (
          <>
            <p className="m-0 me-3">Upload progress: </p>
            <div className="progress" style={{ width: "50%", height: "100%" }}>
              <div
                className="progress-bar progress-bar-striped progress-bar-animated bg-warning text-dark"
                role="progressbar"
                aria-valuenow={uploadProgress}
                aria-valuemin={0}
                aria-valuemax={100}
                style={{ width: `${uploadProgress}%` }}
              >
                {uploadProgress}%
              </div>
            </div>
          </>
        ) : (
          <>
            <button className="btn btn-success" type="submit">
              Save Changes
            </button>
            <button
              className="btn btn-secondary"
              type="button"
              onClick={() => navigate(-1)}
            >
              Cancel
            </button>
          </>
        )}
      </div>
    </form>
  );
};

export default EditEpisode;
