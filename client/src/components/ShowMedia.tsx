import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Media, API_URL } from "../Data";
import { fetchMediaById, deleteMedia } from "../services/MediaService";
import { isAdmin, isAuthenticated } from "../services/UserService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css"; // Importăm fișierul CSS pentru stiluri suplimentare

const ShowMedia: React.FC<{ mediaId: string }> = ({ mediaId }) => {
  const [media, setMedia] = useState<Media | null>(null);
  const navigate = useNavigate();
  const [isAdminUser, setIsAdminUser] = useState(isAdmin());

  const handleDelete = async () => {
    try {
      await deleteMedia(mediaId);
      console.log("Media deleted successfully");
      navigate(-1);
    } catch (error) {
      console.error("Error deleting media:", error);
    }
  };

  useEffect(() => {
    const fetchMedia = async () => {
      try {
        const fetchedMedia = await fetchMediaById(mediaId);

        if (fetchedMedia.posterPath) {
          fetchedMedia.posterPath = API_URL + fetchedMedia.posterPath;
        } else if (fetchedMedia.mediaType === "Episode") {
          if (fetchedMedia.series && fetchedMedia.series.posterPath) {
            fetchedMedia.posterPath = API_URL + fetchedMedia.series.posterPath;
          } else {
            fetchedMedia.posterPath = "/film.png";
          }
        }
        setMedia(fetchedMedia);
      } catch (error) {
        console.error("Error fetching media:", error);
      }
    };

    const checkAuth = () => {
      setIsAdminUser(isAuthenticated());
      setIsAdminUser(isAdmin());
    };

    checkAuth();
    window.addEventListener("storage", checkAuth);
    window.removeEventListener("authChange", checkAuth);

    fetchMedia();

    return () => {
      window.removeEventListener("storage", checkAuth);
      window.addEventListener("authChange", checkAuth);
    };
  }, [mediaId]);

  return (
    <div className="media-page p-5">
      {media ? (
        <>
          {media.mediaType === "Series" ? (
            <ShowSeries {...media} />
          ) : (
            <ShowMovieOrEpisode {...media} />
          )}
          {isAdminUser && (
            <div className="d-flex justify-content-around my-5">
              {media.mediaType == "Series" && (
                <a
                  className="btn btn-primary"
                  href={"/add-episode/" + media.id}
                >
                  Add Episode
                </a>
              )}

              <button
                className="btn btn-danger"
                onClick={() => {
                  const userConfirmed = window.confirm(
                    "Are you sure you want to delete this media?"
                  );

                  if (userConfirmed) {
                    handleDelete();
                  }
                }}
              >
                Delete Media
              </button>
            </div>
          )}
        </>
      ) : (
        <h1>Loading...</h1>
      )}
    </div>
  );
};

const ShowMovieOrEpisode: React.FC<Media> = (media: Media) => {
  const { title, posterPath, releaseDate, description } = media;
  return (
    <div className="container mt-4">
      {media.videoPath && (
        <div className="row justify-content-center mb-4">
          <div className="col-12 col-md-8">
            <video controls className="video-fluid w-100">
              <source src={API_URL + media.videoPath} type="video/mp4" />
              Your browser does not support the video tag.
            </video>
          </div>
        </div>
      )}
      <div className="row">
        <div className="col-md-4">
          <img src={posterPath} alt={title} className="img-fluid rounded" />
        </div>
        <div className="col-md-8">
          <h2>{media.title == "N/A" ? "" : media.title}</h2>
          {media.mediaType == "Episode" &&
            media.episodeNumber &&
            media.seasonNumber && (
              <h3>
                {media.series.title} - Season {media.seasonNumber} / Episode{" "}
                {media.episodeNumber}
              </h3>
            )}
          {media.releaseDate && (
            <p>Release date: {releaseDate?.toLocaleDateString()}</p>
          )}
          <p>{description}</p>
        </div>
      </div>
    </div>
  );
};

const ShowSeries: React.FC<Media> = (series: Media) => {
  const { title, description, posterPath, episodes } = series;
  const [selectedSeason, setSelectedSeason] = useState(1);
  const handleSeasonChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedSeason(parseInt(e.target.value));
  };

  return (
    <div className="media-details">
      <div className="row">
        <div className="col-md-4">
          <img src={posterPath} alt={title} className="img-fluid rounded" />
        </div>
        <div className="col-md-8">
          <h2>{title}</h2>
          <p>{description}</p>
          <div className="season-selector mt-3">
            <label htmlFor="seasonSelect" className="form-label">
              Select Season:
            </label>
            <select
              id="seasonSelect"
              className="form-select"
              value={selectedSeason}
              onChange={handleSeasonChange}
            >
              {Array.from({ length: series.seasons }, (_, index) => (
                <option key={index + 1} value={index + 1}>
                  Season {index + 1}
                </option>
              ))}
            </select>
            {episodes ? (
              <div className="episode-list mt-4 d-flex justify-content-start">
                {episodes.$values
                  .filter(
                    (episode: Media) => episode.seasonNumber === selectedSeason
                  )
                  .sort(
                    (a: Media, b: Media) => a.episodeNumber - b.episodeNumber
                  )
                  .map((episode: Media, index) => (
                    <div key={index}>
                      <a
                        href={"/media/" + episode.id}
                        className="btn btn-dark me-2 mb-2"
                      >
                        Episode {episode.episodeNumber}
                      </a>
                    </div>
                  ))}
              </div>
            ) : (
              <h2 className="p-5">This season has no episodes</h2>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

const MediaPage: React.FC = () => {
  let { id } = useParams();

  return <ShowMedia mediaId={id!} />;
};

export default MediaPage;
