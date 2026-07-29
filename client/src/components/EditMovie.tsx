import React, { useEffect, useState } from "react";
import { editMovie, fetchMediaById } from "../services/MediaService";
import { useNavigate, useParams } from "react-router-dom";
import { Media } from "../Data";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const EditMovie: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [releaseDate, setReleaseDate] = useState("");
  const [posterPath, setPosterPath] = useState("");
  const [videoPath, setVideoPath] = useState("");
  const [image, setImage] = useState<File | null>(null);
  const [video, setVideo] = useState<File | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchMovie = async () => {
      if (id) {
        try {
          const movie = await fetchMediaById(id);
          setTitle(movie.title || "");
          setDescription(movie.description || "");
          setPosterPath(movie.posterPath || "");
          setVideoPath(movie.videoPath || "");
          if (movie.releaseDate) {
            const date = new Date(movie.releaseDate);
            setReleaseDate(date.toISOString().split("T")[0]);
          }
        } catch (error) {
          console.error("Error fetching movie:", error);
        } finally {
          setIsLoading(false);
        }
      }
    };

    fetchMovie();
  }, [id]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    setIsEditing(true);

    const editedMovie: Partial<Media> = {
      title,
      description,
      posterPath,
      videoPath,
      releaseDate: releaseDate ? new Date(releaseDate) : undefined,
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
      console.log("Updating movie...");
      await editMovie(id, editedMovie, image, video, handleUploadProgress);
      console.log("Movie updated successfully");
    } catch (error) {
      console.error("Error editing movie:", error);
      setIsEditing(false);
    }
  };

  useEffect(() => {
    if (uploadProgress === 100) {
      setUploadProgress(0);
      navigate("/mymdb/media/" + id);
    }
  }, [uploadProgress, id, navigate]);

  if (isLoading) {
    return <h1>Loading...</h1>;
  }

  return (
    <form className="create-form" onSubmit={handleSubmit}>
      <h2>Edit Movie</h2>
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
          required
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
          placeholder="e.g., media/videos/movie.mp4"
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

export default EditMovie;
