import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Media, Review, API_URL } from "../Data";
import { fetchMediaById, deleteMedia } from "../services/MediaService";
import {
  isAdmin,
  isAuthenticated,
  getLoggedUser,
} from "../services/UserService";
import { deleteReview } from "../services/ReviewService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

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
    <div className="media-page p-0 pb-5 p-md-1 p-lg-3">
      {media ? (
        <>
          {media.mediaType === "Series" ? (
            <ShowSeries {...media} />
          ) : (
            <ShowMovieOrEpisode {...media} />
          )}
          {isAdminUser && (
            <div className="d-flex justify-content-around mt-5">
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
          {media.reviews && (
            <ShowReviews
              reviewsParam={media.reviews.$values}
              mediaId={mediaId}
            />
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
    <div className="container mt-4">
      <div className="row">
        <div className="col-md-4">
          <img src={posterPath} alt={title} className="img-fluid rounded" />
        </div>
        <div className="col-md-8">
          <h2>{title}</h2>
          <p>{description}</p>
          <div className="season-selector mt-3">
            <select
              id="seasonSelect"
              className="form-select bg-secondary text-white"
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
                  .map((episode: Media) => (
                    <div key={episode.id}>
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

function loggedUserReviewed(reviews: Review[], userId: string) {
  if (!reviews || userId === "null") {
    return false;
  }
  for (let i = 0; i < reviews.length; i++) {
    if (reviews[i].userId && reviews[i].userId === userId) {
      return true;
    }
  }
  return false;
}

const ShowReviews: React.FC<{ reviewsParam: Review[]; mediaId: string }> = ({
  reviewsParam,
  mediaId,
}) => {
  const [reviews, setReviews] = useState<Review[]>(reviewsParam);
  const userId = getLoggedUser();
  const [reset, setReset] = useState(0);

  const handleDelete = () => async () => {
    try {
      await deleteReview(mediaId);
      setReviews(reviews.filter((review) => review.userId !== userId));
      console.log("Review deleted successfully");
      setReset(reset + 1);
    } catch (error) {
      console.error("Error deleting review:", error);
    }
  };

  const userReviewed = loggedUserReviewed(reviews, userId!);
  return (
    <div key={reset} className="review-section pt-5 ms-5 me-5">
      {userReviewed ? (
        <div className="d-flex mb-2">
          <button
            className="me-1 btn btn-danger d-flex justify-content-start"
            onClick={handleDelete()}
          >
            Delete Review
          </button>
        </div>
      ) : (
        <div className="d-flex mb-2">
          <a
            className="me-1 btn btn-secondary d-flex justify-content-start"
            href={userId != null ? "/add-review/" + mediaId : "/login"}
          >
            Add Review
          </a>
        </div>
      )}
      {reviews && reviews.length > 0 ? (
        reviews.map((review) => (
          <div
            key={review.id}
            className={"card text-white mb-2 ".concat(
              review.userId == userId ? "bg-secondary" : "bg-dark"
            )}
          >
            <div className="row g-0 align-items-center flex-column flex-md-row">
              <div className="profile-section col-12 col-md-2 d-flex flex-column align-items-center justify-content-center pt-3">
                {review.userProfile && review.userProfile.profilePicPath ? (
                  <img
                    src={review.userProfile.profilePicPath}
                    alt={review.userProfile.userName}
                    className="img-fluid rounded-circle"
                    style={{
                      width: "80px",
                      height: "80px",
                      objectFit: "cover",
                    }}
                  />
                ) : (
                  <img
                    src={"/profilePic.jpg"}
                    className="img-fluid rounded-circle"
                    style={{
                      width: "80px",
                      height: "80px",
                      objectFit: "cover",
                    }}
                  />
                )}
                <p className="username mt-2 text-center">
                  {review.userProfile && review.userProfile.userName
                    ? review.userId == userId
                      ? "You"
                      : review.userProfile.userName
                    : "Anonymous"}
                </p>
              </div>

              <div className="review-section col-12 col-md-10 d-flex flex-column align-items-start p-3">
                <p className="card-text mb-0">
                  <strong>Rating: {review.rating}/10</strong>
                </p>
                <p className="card-text">{review.comment}</p>
              </div>
            </div>
          </div>
        ))
      ) : (
        <></>
      )}
    </div>
  );
};

const MediaPage: React.FC = () => {
  let { id } = useParams();

  return <ShowMedia mediaId={id!} />;
};

export default MediaPage;
