import React, { useState, useEffect } from "react";
import { Media, API_URL } from "../Data";
import { useNavigate, useLocation } from "react-router";
import { fetchMedia, fetchMovies, fetchSeries } from "../services/MediaService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const ListMedia: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const queryParams = new URLSearchParams(location.search);
  const initialMediaType =
    (queryParams.get("type") as "movies" | "series" | "all") || "all";
  const [mediaType, setMediaType] = useState<"movies" | "series" | "all">(
    initialMediaType
  );
  const [media, setMedia] = useState<Media[]>([]);
  const defaultImage = "film.png";

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

      fetchedMedia.sort((a, b) => {
        return (
          new Date(b.dateCreated).getTime() - new Date(a.dateCreated).getTime()
        );
      });

      const updatedMedia = await Promise.all(
        fetchedMedia.map(async (m) => {
          return {
            ...m,
            posterPath: m.posterPath ? API_URL + m.posterPath : defaultImage,
          };
        })
      );

      setMedia(updatedMedia);
    };

    fetchMediaList();
  }, [mediaType]);

  const handleMediaTypeChange = (type: "movies" | "series" | "all") => {
    setMediaType(type);
    navigate(`?type=${type}`);
  };

  const handleImageSrcError = (
    e: React.SyntheticEvent<HTMLImageElement, Event>
  ) => {
    const img = e.currentTarget as HTMLImageElement;
    img.src = defaultImage;
  };

  return (
    <div className="media-page">
      <div className="btn-group p-3 mb-4 media-buttons" role="group">
        <button
          type="button"
          className={`btn btn-dark ${mediaType === "all" ? "active" : ""}`}
          onClick={() => handleMediaTypeChange("all")}
        >
          All
        </button>
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
      </div>

      <div className="container">
        <div className="row d-flex">
          {media.map((m) => (
            <a
              className="col-6 col-sm-6 col-md-4 col-lg-3 mb-4 text-decoration-none"
              key={m.id}
              href={"/media/" + m.id}
            >
              <div className="card media-card btn btn-dark text-secondary h-100 d-flex flex-column">
                <img
                  src={m.posterPath}
                  onError={handleImageSrcError}
                  alt={m.title}
                  className="card-img-top media-card-img rounded"
                />
                <div className="card-body d-flex justify-content-center align-items-center flex-grow-1">
                  <h5 className="card-title m-0 text-center">{m.title}</h5>
                </div>
              </div>
            </a>
          ))}
        </div>
      </div>
    </div>
  );
};

export default ListMedia;
