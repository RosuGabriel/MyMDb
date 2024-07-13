import React, { useState, useEffect } from "react";
import { Media, API_URL } from "../Data";
import { useNavigate } from "react-router";
import { fetchMedia, fetchMovies, fetchSeries } from "../services/MediaService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const ListMedia: React.FC = () => {
  const [mediaType, setMediaType] = useState<"movies" | "series" | "all">(
    "movies"
  );
  const [media, setMedia] = useState<Media[]>([]);
  const navigate = useNavigate();

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

      fetchedMedia = fetchedMedia.map((m) => ({
        ...m,
        posterPath: m.posterPath ? API_URL + m.posterPath : "film.png",
      }));

      setMedia(fetchedMedia);
    };

    fetchMediaList();
  }, [mediaType]);

  const handleMediaTypeChange = (type: "movies" | "series" | "all") => {
    setMediaType(type);
  };

  const handleClick = (mediaId: string) => {
    navigate("/media/" + mediaId);
  };

  return (
    <div className="media-page">
      <div className="btn-group p-3 mb-3 media-buttons" role="group">
        <button
          type="button"
          className={`btn btn-dark ${mediaType === "movies" ? "active" : ""}`}
          onClick={() => handleMediaTypeChange("movies")}
        >
          Movies
        </button>
        <button
          type="button"
          className={`btn btn-dark ${mediaType === "series" ? "active" : ""}`}
          onClick={() => handleMediaTypeChange("series")}
        >
          Series
        </button>
        <button
          type="button"
          className={`btn btn-dark ${mediaType === "all" ? "active" : ""}`}
          onClick={() => handleMediaTypeChange("all")}
        >
          All
        </button>
      </div>
      <div className="container">
        <div className="row">
          {media.map((m) => (
            <div
              className="col-sm-6 col-md-4 col-lg-3 mb-4"
              key={m.id}
              onClick={() => handleClick(m.id)}
            >
              <div className="card media-card btn btn-dark text-white">
                <img
                  src={m.posterPath}
                  alt={m.title}
                  className="card-img-top media-card-img"
                />
                <div className="card-body">
                  <h5 className="card-title">{m.title}</h5>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default ListMedia;
