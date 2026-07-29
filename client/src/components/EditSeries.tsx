import React, { useEffect, useState } from "react";
import { editSeries, fetchMediaById } from "../services/MediaService";
import { useNavigate, useParams } from "react-router-dom";
import { Media } from "../Data";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const EditSeries: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [releaseDate, setReleaseDate] = useState("");
  const [posterPath, setPosterPath] = useState("");
  const [seasons, setSeasons] = useState(1);
  const [image, setImage] = useState<File | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchSeries = async () => {
      if (id) {
        try {
          const series = await fetchMediaById(id);
          setTitle(series.title || "");
          setDescription(series.description || "");
          setPosterPath(series.posterPath || "");
          if (series.releaseDate) {
            const date = new Date(series.releaseDate);
            setReleaseDate(date.toISOString().split("T")[0]);
          }
          setSeasons(series.seasons || 1);
        } catch (error) {
          console.error("Error fetching series:", error);
        } finally {
          setIsLoading(false);
        }
      }
    };

    fetchSeries();
  }, [id]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    setIsEditing(true);

    const editedSeries: Partial<Media> = {
      title,
      description,
      posterPath,
      releaseDate: releaseDate ? new Date(releaseDate) : undefined,
      seasons,
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
      console.log("Updating series...");
      await editSeries(id, editedSeries, image, handleUploadProgress);
      console.log("Series updated successfully");
    } catch (error) {
      console.error("Error editing series:", error);
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
      <h2>Edit Series</h2>
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
        <label htmlFor="seasons" className="form-label">
          Seasons
        </label>
        <input
          className="form-control"
          type="number"
          id="seasons"
          value={seasons}
          onChange={(e) => setSeasons(parseInt(e.target.value))}
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

export default EditSeries;
