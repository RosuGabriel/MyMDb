import React, { useState } from "react";
import { createMovie } from "../services/MediaService";
import { useNavigate } from "react-router-dom";
import { Media } from "../Data";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const CreateMovieForm: React.FC = () => {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [releaseDate, setReleaseDate] = useState("");
  const [image, setImage] = useState<File | null>(null);
  const [video, setVideo] = useState<File | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsCreating(true);

    const newMovie: Partial<Media> = {
      title,
      description,
      releaseDate: releaseDate ? new Date(releaseDate) : undefined,
    };

    try {
      const createdMovie = await createMovie(newMovie, image, video);
      console.log("Movie created successfully:", createdMovie);
      navigate("/media");
    } catch (error) {
      console.error("Error creating movie:", error);
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <form className="create-form" onSubmit={handleSubmit}>
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
        <label htmlFor="image" className="form-label">
          Poster
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
        <label htmlFor="video" className="form-label">
          Video
        </label>
        <input
          className="form-control"
          type="file"
          id="video"
          accept="video/*"
          onChange={(e) => setVideo(e.target.files?.[0] || null)}
        />
      </div>
      <button className="btn btn-success" type="submit" disabled={isCreating}>
        {isCreating ? "Creating..." : "Create Movie"}
      </button>
    </form>
  );
};

export default CreateMovieForm;
