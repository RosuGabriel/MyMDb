import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Media, API_URL } from "../Data";
import { fetchMediaById, deleteMedia } from "../services/MediaService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css"; // Importăm fișierul CSS pentru stiluri suplimentare

interface ShowMediaProps {
  mediaId: string;
}

const ShowMedia: React.FC<ShowMediaProps> = ({ mediaId }) => {
  const [media, setMedia] = useState<Media | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchMedia = async () => {
      try {
        const fetchedMedia = await fetchMediaById(mediaId);

        fetchedMedia.posterPath = fetchedMedia.posterPath
          ? API_URL + fetchedMedia.posterPath
          : "/film.png";
        setMedia(fetchedMedia);
      } catch (error) {
        console.error("Error fetching media:", error);
      }
    };

    fetchMedia();
  }, [mediaId]);

  return (
    <div className="media-page">
      {media ? (
        media.mediaType == "Series" ? (
          <>
            <ShowSeries {...media} />
            <br />
            <br />
            <button
              className="btn btn-danger my-3"
              onClick={() => {
                deleteMedia(media.id);
                navigate("/media");
              }}
            >
              Delete Media
            </button>
          </>
        ) : (
          <>
            <ShowMovieOrEpisode {...media} />
            <br />
            <br />
            <button
              className="btn btn-danger my-3"
              onClick={() => {
                const userConfirmed = window.confirm(
                  "Are you sure you want to delete this media?"
                );

                if (userConfirmed) {
                  deleteMedia(media.id);
                  navigate("/media");
                }
              }}
            >
              Delete Media
            </button>
          </>
        )
      ) : (
        <h1>Loading...</h1>
      )}
    </div>
  );
};

const ShowMovieOrEpisode: React.FC<Media> = (media: Media) => {
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
          <img
            src={media.posterPath}
            alt={media.title}
            className="img-fluid rounded"
          />
        </div>
        <div className="col-md-8">
          <h2>{media.title}</h2>
          {media.releaseDate && (
            <p>
              Release date: {new Date(media.releaseDate).toLocaleDateString()}
            </p>
          )}
          {/* {media.rating && <p>Rating: {media.rating}</p>} */}
          <p>{media.description}</p>
        </div>
      </div>
    </div>
  );
};
const ShowSeries: React.FC<Media> = (series: Media) => {
  return (
    <div className="media-details">
      <h2>{series.title}</h2>
      <p>{series.description}</p>
      {series.releaseDate && (
        <p>Release date: {series.releaseDate?.toString()}</p>
      )}
      <img
        src={series.posterPath}
        alt={series.title}
        className="img-fluid rounded"
      />
    </div>
  );
};

const MediaPage: React.FC = () => {
  let { id } = useParams<{ id: string }>();

  return <ShowMedia mediaId={id!} />;
};

export default MediaPage;
