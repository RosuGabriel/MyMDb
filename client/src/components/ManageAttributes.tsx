import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Media, Attribute } from "../Data";
import { fetchMediaById } from "../services/MediaService";
import { deleteAttribute } from "../services/AttributeService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

const ManageAttributes: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [media, setMedia] = useState<Media | null>(null);
  const [attributes, setAttributes] = useState<Attribute[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchMedia = async () => {
      try {
        const fetchedMedia = await fetchMediaById(id!);
        setMedia(fetchedMedia);

        // Extract attributes from the media
        if (fetchedMedia.mediaAttributes?.$values) {
          setAttributes(fetchedMedia.mediaAttributes.$values);
        }
        setLoading(false);
      } catch (err) {
        setError("Error loading media attributes");
        setLoading(false);
      }
    };

    fetchMedia();
  }, [id]);

  const handleDeleteAttribute = async (attribute: Attribute) => {
    const confirmed = window.confirm(
      `Delete ${attribute.type} (${attribute.language})?`,
    );
    if (!confirmed) return;

    try {
      await deleteAttribute(
        attribute.mediaId,
        attribute.type,
        attribute.language,
      );
      setAttributes(
        attributes.filter(
          (a) =>
            !(
              a.mediaId === attribute.mediaId &&
              a.type === attribute.type &&
              a.language === attribute.language
            ),
        ),
      );
    } catch (err) {
      setError("Error deleting attribute");
    }
  };

  const handleEditAttribute = (attribute: Attribute) => {
    navigate(`/mymdb/edit-attribute/${id}`, {
      state: { attribute },
    });
  };

  const handleAddAttribute = () => {
    navigate(`/mymdb/add-attribute/${id}`);
  };

  if (loading) {
    return <div className="p-5">Loading...</div>;
  }

  if (!media) {
    return <div className="p-5">Media not found</div>;
  }

  return (
    <div className="p-4 p-md-5">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2>{media.title}</h2>
          <h5 className="text-white">Attributes</h5>
        </div>
        <button
          className="btn btn-secondary"
          onClick={() => navigate(`/mymdb/media/${id}`)}
        >
          Back to Media
        </button>
      </div>

      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      {attributes.length === 0 ? (
        <div className="alert alert-info" role="alert">
          No attributes yet. Add one to get started!
        </div>
      ) : (
        <div className="mb-5">
          <div className="row">
            {attributes.map((attribute, index) => (
              <div key={index} className="col-md-6 col-lg-4 mb-3">
                <div className="card h-100 card-bg">
                  <div className="card-body d-flex flex-column">
                    <div className="mb-3 flex-grow-1">
                      <h6 className="card-title mb-1 text-white">
                        {attribute.type} - {attribute.language}
                      </h6>
                    </div>
                    <div className="d-flex gap-2">
                      <button
                        className="btn btn-sm btn-primary flex-grow-1"
                        onClick={() => handleEditAttribute(attribute)}
                      >
                        Edit
                      </button>
                      <button
                        className="btn btn-sm btn-danger"
                        onClick={() => handleDeleteAttribute(attribute)}
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="d-grid gap-2 col-md-4">
        <button className="btn btn-success btn-lg" onClick={handleAddAttribute}>
          Add New Attribute
        </button>
      </div>
    </div>
  );
};

export default ManageAttributes;
