import React, { useState, useEffect } from "react";
import { Media } from "../Data";
import { useNavigate, useLocation } from "react-router";
import { fetchMedia } from "../services/MediaService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";
import MediaItem from "./MediaItem";
import ContinueWatching from "./ContinueWatching";

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
      let fetchedMedia: Media[] = await fetchMedia();
      const searchQuery = queryParams.get("search");

      fetchedMedia.sort((a, b) => {
        return (
          new Date(b.dateCreated).getTime() - new Date(a.dateCreated).getTime()
        );
      });

      let updatedMedia = fetchedMedia;
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
    if (mediaType === "movies" || mediaType === "series") {
      document.title =
        "MyMDb - " + mediaType.charAt(0).toUpperCase() + mediaType.slice(1);
    } else {
      document.title = "MyMDb";
    }
    if (mediaType === "movies") {
      filteredMedia = filteredMedia.filter((m) => m.mediaType === "Movie");
    } else if (mediaType === "series") {
      filteredMedia = filteredMedia.filter((m) => m.mediaType === "Series");
    }

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

  return (
    <div className="media-page">
      <div className="btn-group p-3 my-2 media-buttons" role="group">
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
        <ContinueWatching />
        <div className="row d-flex">
          {media.map((m) => (
            <MediaItem key={m.id} media={m} defaultImage={defaultImage} />
          ))}
        </div>
      </div>
    </div>
  );
};

export default ListMedia;
