import React from "react";
import { Media } from "../Data";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";
import ImageDisplay from "./ImageDisplay";

const MediaItem: React.FC<{ media: Media; defaultImage: string }> = ({
  media,
}) => {
  return (
    <a
      className="col-6 col-sm-4 col-md-4 col-lg-3 col-xl-2 mb-4 text-decoration-none"
      href={"/mymdb/media/" + media.id}
    >
      <div className="card media-card btn btn-dark text-secondary h-100 d-flex flex-column p-1">
        <ImageDisplay
          src={media.posterPath}
          className="card-img-top media-card-img rounded"
          alt={media.title}
          backupImagePath="/film.png"
        />
        {/* <div className="card-body d-flex justify-content-center align-items-center flex-grow-1">
          <h5 className="card-title m-0 text-center">{media.title}</h5>
        </div> */}
      </div>
    </a>
  );
};

export default MediaItem;
