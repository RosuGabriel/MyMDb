import React, { useState, useEffect } from "react";
import { Media } from "../Data";
import { fetchMedia } from "../services/MediaService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";
import MediaItem from "./MediaItem";

const ContinueWatching: React.FC = () => {
  const [watchingItems, setWatchingItems] = useState<Media[]>([]);

  useEffect(() => {
    const fetchMediaList = async () => {
      let fetchedMedia: Media[] = await fetchMedia();
      fetchedMedia = fetchedMedia.sort((a, b) => {
        return (
          new Date(b.dateCreated).getTime() - new Date(a.dateCreated).getTime()
        );
      });
      setWatchingItems(fetchedMedia);
    };

    fetchMediaList();
  }, []);

  return (
    <>
      <div
        className="d-flex overflow-auto bg-warning mb-3 p-2 rounded"
        style={{ whiteSpace: "nowrap", gap: "15px" }}
      >
        {watchingItems.map((media) => (
          <MediaItem key={media.id} media={media} defaultImage="/film.png" />
        ))}
      </div>
    </>
  );
};

export default ContinueWatching;
