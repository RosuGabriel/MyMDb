import { useParams, useLocation } from "react-router-dom";
import React, { useRef, useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Media, Review, API_URL, IContinueWatching } from "../Data";
import { fetchMediaById, deleteMedia } from "../services/MediaService";
import {
  isAdmin,
  isAuthenticated,
  getLoggedUser,
} from "../services/UserService";
import { deleteReview } from "../services/ReviewService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";
import VideoPlayer from "./VideoPlayer";
import ImageDisplay from "./ImageDisplay";
import {
  addContinueWatching,
  deleteContinueWatching,
  fetchContinueWatchingById,
} from "../services/ContinueWatchingService";

const ShowMedia: React.FC<{ mediaId: string; season: number }> = ({
  mediaId,
  season,
}) => {
  const [media, setMedia] = useState<Media | null>(null);
  const [selectedSeason, setSelectedSeason] = useState(season);
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
        document.title = fetchedMedia!.title;
        if (fetchedMedia.posterPath) {
          fetchedMedia.posterPath =
            API_URL + "static/" + fetchedMedia.posterPath;
        } else if (fetchedMedia.mediaType === "Episode") {
          if (fetchedMedia.series && fetchedMedia.series.posterPath) {
            fetchedMedia.posterPath =
              API_URL + "static/" + fetchedMedia.series.posterPath;
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
    window.addEventListener("authChange", checkAuth);

    fetchMedia();

    return () => {
      window.removeEventListener("storage", checkAuth);
      window.removeEventListener("authChange", checkAuth);
    };
  }, [mediaId]);

  useEffect(() => {
    setSelectedSeason(season);
  }, [season]);

  return (
    <div className="media-page p-0 pb-5 p-md-1 p-lg-3">
      {media ? (
        <>
          {media.mediaType === "Series" ? (
            <ShowSeries {...media} selectedSeason={selectedSeason} />
          ) : (
            <ShowMovieOrEpisode {...media} />
          )}
          {isAdminUser && (
            <div className="d-flex justify-content-around mt-5">
              {media.mediaType == "Series" && (
                <a
                  className="btn btn-primary"
                  href={"/mymdb/add-episode/" + media.id}
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
              {media.mediaType !== "Series" && (
                <a
                  className="btn btn-primary"
                  href={"/mymdb/add-attribute/" + media.id}
                >
                  Add Attribute
                </a>
              )}
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
  const attributes = media.mediaAttributes && media.mediaAttributes.$values;
  const [episodes, setEpisodes] = useState<Media[]>([]);
  const [prevEpisodeId, setPrevEpisodeId] = useState<string | null>(null);
  const [nextEpisodeId, setNextEpisodeId] = useState<string | null>(null);
  const mediaKeys = ["ArrowRight", "ArrowLeft", " ", "f", "m"];
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const [isPaused, setIsPaused] = useState(true);

  const getContinueWatching = async (
    mediaId: string,
    episodeId: string | undefined
  ): Promise<IContinueWatching> => {
    try {
      return await fetchContinueWatchingById(mediaId, episodeId);
    } catch (error) {
      console.error("Error fetching continue watching:", error);
      return {} as IContinueWatching;
    }
  };

  const setWatchedTime = async (
    mediaId: string,
    episodeId: string | undefined
  ) => {
    const cw = await getContinueWatching(mediaId, episodeId);
    videoRef.current!.currentTime = cw.watchedTime ?? 0;
  };

  const updateContinueWatching = async (
    mediaId: string,
    episodeId: string | undefined
  ) => {
    if (videoRef.current) {
      const video = videoRef.current;
      const watchedTime = Math.floor(video.currentTime) || 0;
      const duration = Math.floor(video.duration) || 0;
      if (watchedTime > 0 && duration > 0) {
        if (watchedTime >= duration - 90) {
          deleteContinueWatching({ mediaId: mediaId, episodeId: episodeId });
          if (nextEpisodeId) {
            addContinueWatching({
              mediaId: mediaId,
              episodeId: nextEpisodeId,
              watchedTime: 0,
              duration: 10,
            });
          }
        } else {
          const continueWatching: Partial<IContinueWatching> = {
            mediaId: mediaId,
            episodeId: episodeId,
            watchedTime: watchedTime,
            duration: duration,
          };
          addContinueWatching(continueWatching);
        }
      }
    }
  };

  // Setting watched time on load and handling pause/play events
  useEffect(() => {
    let mediaId = undefined;
    let episodeId = undefined;
    if (media.mediaType === "Movie") {
      mediaId = media.id;
    } else {
      mediaId = media.series?.id;
      episodeId = media.id;
    }

    setWatchedTime(mediaId, episodeId);

    const video = videoRef.current;
    if (!video) return;

    const handlePause = () => setIsPaused(true);
    const handlePlay = () => setIsPaused(false);

    video.addEventListener("pause", handlePause);
    video.addEventListener("play", handlePlay);

    return () => {
      video.removeEventListener("pause", handlePause);
      video.removeEventListener("play", handlePlay);
    };
  }, []);

  // Periodically updating watched time
  useEffect(() => {
    let mediaId = undefined;
    let episodeId = undefined;
    if (media.mediaType === "Movie") {
      mediaId = media.id;
    } else {
      mediaId = media.series?.id;
      episodeId = media.id;
    }

    const delayedUpdate = setTimeout(() => {
      updateContinueWatching(mediaId, episodeId);
    }, 3000);

    const periodicSetWatchProgress = setInterval(() => {
      updateContinueWatching(mediaId, episodeId);
    }, 45000);

    return () => {
      clearInterval(periodicSetWatchProgress);
      clearTimeout(delayedUpdate);
    };
  }, [nextEpisodeId, isPaused]);

  // Fetching other episodes of the series
  useEffect(() => {
    const getEpisodes = async () => {
      try {
        const fetchedEpisodes = (await fetchMediaById(media.series?.id))
          .episodes.$values;
        document.title =
          title !== "N/A"
            ? title
            : `${media.series?.title} - S${media.seasonNumber} - E${media.episodeNumber}`;
        setEpisodes(fetchedEpisodes);
      } catch (error) {
        console.error("Error fetching other episodes:", error);
      }
    };

    if (media.series?.id) {
      getEpisodes();
    }
  }, [media.series?.id]);

  // Setting next and previous episode ids
  useEffect(() => {
    const currentSeasonEpisodes = episodes.filter(
      (ep) => ep.seasonNumber === media.seasonNumber
    );

    const prevEpisode = currentSeasonEpisodes.find(
      (ep) => ep.episodeNumber === media.episodeNumber - 1
    );

    if (!prevEpisode && media.seasonNumber > 1) {
      const prevSeasonEpisodes = episodes.filter(
        (ep) => ep.seasonNumber === media.seasonNumber - 1
      );

      const lastEpisodeOfPrevSeason = prevSeasonEpisodes.at(-1) || null;

      setPrevEpisodeId(lastEpisodeOfPrevSeason?.id || null);
    } else {
      setPrevEpisodeId(prevEpisode?.id || null);
    }

    const nextEpisode = currentSeasonEpisodes.find(
      (ep) => ep.episodeNumber === (media.episodeNumber || 0) + 1
    );

    if (!nextEpisode && media.seasonNumber < (media.series?.seasons || 0)) {
      const nextSeasonEpisode = episodes.find(
        (ep) =>
          ep.seasonNumber === media.seasonNumber + 1 && ep.episodeNumber === 1
      );

      setNextEpisodeId(nextSeasonEpisode?.id || null);
    } else {
      setNextEpisodeId(nextEpisode?.id || null);
    }
  }, [episodes, media.seasonNumber, media.episodeNumber]);

  // Handling keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      const video = videoRef.current;
      if (!video) {
        return;
      }

      if (mediaKeys.includes(event.key)) {
        event.preventDefault();
      }

      switch (event.key) {
        case "ArrowRight":
          video.currentTime += 5;
          break;
        case "ArrowLeft":
          video.currentTime -= 5;
          break;
        case " ":
          if (video.paused) {
            video.play();
          } else {
            video.pause();
          }
          break;
        case "f":
          if (!document.fullscreenElement) {
            video.requestFullscreen();
          } else {
            document.exitFullscreen();
          }
          break;
        case "m":
          video.muted = !video.muted;
          break;
        default:
          break;
      }
    };

    window.addEventListener("keydown", handleKeyDown, { capture: true });

    return () => {
      window.removeEventListener("keydown", handleKeyDown, { capture: true });
    };
  }, []);

  return (
    <div className="container mt-4">
      {media.videoPath && (
        <div className="row justify-content-center mb-4">
          <div className="col-12 col-md-8">
            <video
              ref={videoRef}
              src={`${API_URL}static/${media.videoPath}`}
              controls
              className="w-100 rounded"
            >
              {attributes
                .filter((attr) => attr.type.toLowerCase() === "subtitle")
                .map((attribute) => (
                  <track
                    key={attribute.language}
                    src={`${API_URL}static/${attribute.attributePath}`}
                    kind="subtitles"
                    srcLang={attribute.language}
                    label={attribute.language}
                  />
                ))}
            </video>
            {/* <VideoPlayer
              ref={videoRef}
              src={media.videoPath}
              attributes={attributes}
            /> */}
          </div>
        </div>
      )}
      <div className="row">
        <div className="col-md-8 bg-dark rounded mb-4">
          <br />
          <h2>{title === "N/A" ? "" : title}</h2>
          {media.mediaType === "Episode" &&
            media.episodeNumber &&
            media.seasonNumber && (
              <>
                <div
                  className="btn-group mb-3"
                  role="group"
                  aria-label="Episodes navigation"
                >
                  {prevEpisodeId && (
                    <a
                      href={`/mymdb/media/${prevEpisodeId}`}
                      className="btn btn-secondary me-2"
                    >
                      Prev Episode
                    </a>
                  )}
                  {nextEpisodeId && (
                    <a
                      href={`/mymdb/media/${nextEpisodeId}`}
                      className="btn btn-secondary"
                    >
                      Next Episode
                    </a>
                  )}
                </div>
                <h3>{media.series.title} </h3>
                <h4>
                  Season {media.seasonNumber} | Episode {media.episodeNumber}
                </h4>
              </>
            )}
          {releaseDate && (
            <p>Release date: {releaseDate.toLocaleDateString()}</p>
          )}
          <p>{description}</p>
        </div>
        <div className="col-md-4 mb-4">
          <ImageDisplay
            src={posterPath}
            alt={title}
            className="img-fluid rounded"
            backupImagePath="/film.png"
          />
        </div>
      </div>
    </div>
  );
};

const ShowSeries: React.FC<Media & { selectedSeason: number }> = (series) => {
  const { title, description, posterPath, episodes, selectedSeason } = series;
  const [season, setSeason] = useState(selectedSeason);
  const navigate = useNavigate();

  const handleSeasonChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newSeason = parseInt(e.target.value);
    setSeason(newSeason);
    navigate(`/mymdb/media/${series.id}?season=${newSeason}`);
  };

  return (
    <div className="container mt-4">
      <div className="row">
        <div className="col-md-8">
          <br />
          <h2>{title}</h2>
          <p>{description}</p>
          <div className="season-selector mt-3">
            <select
              id="seasonSelect"
              className="form-select bg-secondary text-white"
              value={season}
              onChange={handleSeasonChange}
            >
              {Array.from({ length: series.seasons }, (_, index) => (
                <option key={index + 1} value={index + 1}>
                  Season {index + 1}
                </option>
              ))}
            </select>
            {episodes &&
            episodes.$values &&
            episodes.$values.filter(
              (episode: Media) => episode.seasonNumber === season
            ).length ? (
              <div className="container mt-4">
                <div className="episode-list d-flex row">
                  {episodes.$values
                    .filter((episode: Media) => episode.seasonNumber === season)
                    .sort(
                      (a: Media, b: Media) => a.episodeNumber - b.episodeNumber
                    )
                    .map((episode: Media) => (
                      <div
                        key={episode.id}
                        className="col-6 col-sm-3 col-md-3 col-lg-3 mb-2"
                      >
                        <a
                          href={`/mymdb/media/${episode.id}`}
                          className="btn btn-dark w-100"
                        >
                          Episode {episode.episodeNumber}
                        </a>
                      </div>
                    ))}
                </div>
              </div>
            ) : (
              <h5 className="mt-4 d-flex justify-content-start">
                🤔 Looks like there are no episodes for this season
              </h5>
            )}
          </div>
        </div>
        <div className="col-md-4">
          <ImageDisplay
            src={posterPath}
            alt={title}
            className="img-fluid rounded"
            backupImagePath="/film.png"
          />
          {/* <img src={posterPath} alt={title} className="img-fluid rounded" /> */}
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
            href={
              userId != null ? "/mymdb/add-review/" + mediaId : "/mymdb/login"
            }
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
                <ImageDisplay
                  src={review.userProfile.profilePicPath}
                  alt={review.userProfile.userName}
                  className="img-fluid rounded-circle"
                  style={{
                    width: "80px",
                    height: "80px",
                    objectFit: "cover",
                  }}
                />
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
  let { id } = useParams<{ id: string }>();
  let query = new URLSearchParams(useLocation().search);
  let season = query.get("season");

  return <ShowMedia mediaId={id!} season={season ? parseInt(season) : 1} />;
};

export default MediaPage;
