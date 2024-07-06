import React, { useState, useEffect } from "react";
import { Media } from "../Models";
import { fetchMedia, fetchMovies, fetchSeries } from "../services/MediaService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css"; // Importăm fișierul CSS pentru stiluri suplimentare

const ListMedia: React.FC = () => {
  const [mediaType, setMediaType] = useState<"movies" | "series" | "all">(
    "all"
  );
  const [media, setMedia] = useState<Media[]>([]);

  useEffect(() => {
    const fetchMediaList = async () => {
      let fetchedMedia: Media[] = [];

      if (mediaType === "all") {
        fetchedMedia = await fetchMedia();
      } else if (mediaType === "movies") {
        fetchedMedia = await fetchMovies();
      } else if (mediaType === "series") {
        fetchedMedia = await fetchSeries();
      }

      setMedia(fetchedMedia);
    };

    fetchMediaList();
  }, [mediaType]);

  const handleMediaTypeChange = (type: "movies" | "series" | "all") => {
    setMediaType(type);
  };

  return (
    <>
      <div className="btn-group mb-3" role="group">
        <button
          type="button"
          className={`btn btn-primary ${
            mediaType === "movies" ? "active" : ""
          }`}
          onClick={() => handleMediaTypeChange("movies")}
        >
          Movies
        </button>
        <button
          type="button"
          className={`btn btn-primary ${
            mediaType === "series" ? "active" : ""
          }`}
          onClick={() => handleMediaTypeChange("series")}
        >
          Series
        </button>
        <button
          type="button"
          className={`btn btn-primary ${mediaType === "all" ? "active" : ""}`}
          onClick={() => handleMediaTypeChange("all")}
        >
          All
        </button>
      </div>
      <div className="container">
        <div className="row">
          {media.map((m) => (
            <div className="col-sm-6 col-md-4 col-lg-3 mb-4" key={m.id}>
              <div className="card media-card">
                {m.posterPath && (
                  <img
                    src={"https://localhost:7292/" + m.posterPath}
                    alt={m.title}
                    className="card-img-top media-card-img"
                  />
                )}
                <div className="card-body">
                  <h5 className="card-title">{m.title}</h5>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </>
  );
};

export default ListMedia;
