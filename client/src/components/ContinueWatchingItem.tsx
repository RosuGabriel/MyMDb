import React from "react";
import { IContinueWatching } from "../Data";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";
import ImageDisplay from "./ImageDisplay";
import { deleteContinueWatching } from "../services/ContinueWatchingService";

const ContinueWatchingItem: React.FC<{
  cw: IContinueWatching;
  defaultImage: string;
  onDelete: (id: string) => void;
}> = ({ cw, onDelete }) => {
  function handleDelete() {
    console.log("Delete media with id: ", cw.mediaId, "from continue watching");
    deleteContinueWatching(cw);
    onDelete(cw.mediaId);
  }

  return (
    <div className="media-card d-flex flex-column overflow-hidden my-1 rounded col-5 col-sm-4 col-md-4 col-lg-3 col-xl-2 text-decoration-none">
      <a
        href={
          "/mymdb/media/" + (cw.episodeId !== null ? cw.episodeId : cw.mediaId)
        }
        className="card media-card btn btn-dark text-secondary h-100 p-1"
      >
        <div className="position-relative">
          <ImageDisplay
            src={cw.posterPath}
            className="card-img-top media-card-img rounded"
            backupImagePath="/film.png"
          />
          <div className="position-absolute bottom-0 bg-dark text-white w-100 p-0 m-0 opacity-75">
            {cw.episodeId && (
              <p className="m-0 p-0">
                S{cw.seasonNumber}-E{cw.episodeNumber}
              </p>
            )}
            <div
              className="progress bg-secondary m-0 p-0"
              style={{ width: "100%", height: "8px" }}
            >
              <div
                className="progress-bar bg-primary"
                role="progressbar"
                aria-valuenow={cw.watchedTime}
                aria-valuemin={0}
                aria-valuemax={cw.duration}
                style={{ width: `${(cw.watchedTime / cw.duration) * 100}%` }}
              >
                &nbsp;
              </div>
            </div>
          </div>
        </div>
      </a>

      <button
        className="media-card btn btn-dark text-warning m-1 p-0 border-0"
        type="button"
        onClick={handleDelete}
      >
        ✕
      </button>
    </div>
  );
};

export default ContinueWatchingItem;
