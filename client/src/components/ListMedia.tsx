import React, { useState, useEffect } from "react";
import { Media, API_URL } from "../Data";
import { useNavigate, useLocation } from "react-router";
import { fetchMedia } from "../services/MediaService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const ListMedia: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const queryParams = new URLSearchParams(location.search);
  const defaultImage = "film.png";

  const initialMediaType =
    (queryParams.get("type") as "movies" | "series" | "all") || "all";

  const [mediaType, setMediaType] = useState<"movies" | "series" | "all">(
    initialMediaType
  );

  const [allMedia, setAllMedia] = useState<Media[]>([]);

  const [media, setMedia] = useState<Media[]>([]);

  // Fetch media list
  useEffect(() => {
    const fetchMediaList = async () => {
      let fetchedMedia: Media[] = [];

      fetchedMedia = await fetchMedia();
      const searchQuery = queryParams.get("search");

      fetchedMedia.sort((a, b) => {
        return (
          new Date(b.dateCreated).getTime() - new Date(a.dateCreated).getTime()
        );
      });

      let updatedMedia = await Promise.all(
        fetchedMedia.map(async (m) => {
          return {
            ...m,
            posterPath: m.posterPath ? API_URL + m.posterPath : defaultImage,
          };
        })
      );

      setAllMedia(updatedMedia);

      // Filter by media type
      if (mediaType === "movies") {
        updatedMedia = updatedMedia.filter((m) => m.mediaType === "Movie");
      } else if (mediaType === "series") {
        updatedMedia = updatedMedia.filter((m) => m.mediaType === "Series");
      }

      // Filter by search query
      if (searchQuery && searchQuery.trim()) {
        updatedMedia = updatedMedia.filter((m) =>
          m.title.toLowerCase().includes(searchQuery.toLowerCase())
        );
      }

      setMedia(updatedMedia);
    };

    fetchMediaList();
  }, []);

  // Filter media
  useEffect(() => {
    let filteredMedia = allMedia;
    const searchQuery = queryParams.get("search");

    // Filter by media type
    if (mediaType === "movies") {
      filteredMedia = filteredMedia.filter((m) => m.mediaType === "Movie");
    } else if (mediaType === "series") {
      filteredMedia = filteredMedia.filter((m) => m.mediaType === "Series");
    }

    // Filter by search query
    if (searchQuery && searchQuery.trim()) {
      filteredMedia = filteredMedia.filter((m) =>
        m.title.toLowerCase().includes(searchQuery.toLowerCase())
      );
    }

    setMedia(filteredMedia);
  }, [mediaType, location.search, allMedia]);

  const handleMediaTypeChange = (type: "movies" | "series" | "all") => {
    setMediaType(type);
    queryParams.set("type", type);
    navigate({ search: queryParams.toString() });
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

      <div className="container-fluid">
        <div className="row d-flex">
          {media.map((m) => (
            <a
              className="col-6 col-sm-4 col-md-4 col-lg-3 col-xl-2 mb-4 text-decoration-none"
              key={m.id}
              href={"/media/" + m.id}
            >
              <div className="card media-card btn btn-dark text-secondary h-100 d-flex flex-column p-1">
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
