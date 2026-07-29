import React, { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { Attribute, Languages } from "../Data";
import { updateAttribute } from "../services/AttributeService";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";

interface LocationState {
  attribute?: Attribute;
}

const EditAttribute: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as LocationState;

  const [language, setLanguage] = useState<string>("");
  const [type, setType] = useState<string>("");
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (state?.attribute) {
      setLanguage(state.attribute.language);
      setType(state.attribute.type);
      setLoading(false);
    } else {
      setError("Attribute not found. Please go back and try again.");
      setLoading(false);
    }
  }, [state]);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!state?.attribute) return;

    try {
      const attributeToUpdate: Partial<Attribute> = {
        mediaId: state.attribute.mediaId,
        type,
        language,
      };
      await updateAttribute(attributeToUpdate, file);
      navigate(`/mymdb/manage-attributes/${id}`);
    } catch (err) {
      setError("Error updating attribute");
      console.error(err);
    }
  };

  if (loading) {
    return <div className="p-5">Loading...</div>;
  }

  if (error) {
    return (
      <div className="p-5">
        <div className="alert alert-danger">{error}</div>
        <button
          className="btn btn-secondary"
          onClick={() => navigate(`/mymdb/manage-attributes/${id}`)}
        >
          Go Back
        </button>
      </div>
    );
  }

  return (
    <div className="p-5">
      <div className="mb-4">
        <h2>Edit Attribute</h2>
        <h5 className="text-white">
          {type} ({language})
        </h5>
        {state?.attribute?.attributePath && (
          <p className="text-white small text-break">
            {state.attribute.attributePath}
          </p>
        )}
      </div>

      <form onSubmit={handleSubmit} className="col-md-6">
        <div className="mb-3">
          <label htmlFor="type" className="form-label">
            Type
          </label>
          <select
            className="form-control"
            value={type}
            id="type"
            onChange={(e) => setType(e.target.value)}
            required
          >
            <option>Subtitle</option>
            <option>Dubbing</option>
          </select>
        </div>

        <div className="mb-3">
          <label htmlFor="language" className="form-label">
            Language
          </label>
          <select
            className="form-control"
            id="language"
            value={language}
            onChange={(e) => setLanguage(e.target.value)}
            required
          >
            {Languages.map((lang) => (
              <option key={lang} value={lang}>
                {lang}
              </option>
            ))}
          </select>
        </div>

        <div className="mb-3">
          <label htmlFor="file" className="form-label">
            File (optional - leave blank to keep existing)
          </label>
          <input
            className="form-control"
            type="file"
            id="file"
            onChange={(e) => setFile(e.target.files?.[0] || null)}
          />
        </div>

        <div className="d-flex gap-2">
          <button className="btn btn-primary" type="submit">
            Update Attribute
          </button>
          <button
            className="btn btn-secondary"
            type="button"
            onClick={() => navigate(`/mymdb/manage-attributes/${id}`)}
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
};

export default EditAttribute;
