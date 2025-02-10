import React, { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Attribute, Languages } from "../Data";
import { addAttribute } from "../services/AttributeService";

interface AddAttributeProps {
  mediaId: string;
}

const CreateAttribute: React.FC<AddAttributeProps> = ({ mediaId }) => {
  const [language, setLanguage] = useState<string>("English");
  const [type, setType] = useState<string>("Subtitle");
  const [file, setFile] = useState<File | null>(null);
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const newAttribute: Partial<Attribute> = { mediaId, type, language };
    try {
      const addedAttribute = await addAttribute(newAttribute, file);
      console.log("Attribute added successfully:", addedAttribute);
      navigate(`/mymdb/media/${mediaId}`);
    } catch (error) {
      console.error("Error adding attribute:", error);
    }
  };

  return (
    <div className="p-5">
      <h2>Add Attribute</h2>
      <form onSubmit={handleSubmit}>
        <div className="mb-3">
          <label htmlFor="type" className="form-label">
            Type:
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
            Language:
          </label>
          <select
            className="form-control"
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
            File
          </label>
          <input
            className="form-control"
            type="file"
            id="file"
            onChange={(e) => setFile(e.target.files?.[0] || null)}
            required
          />
        </div>
        <button type="submit" className="btn btn-success">
          Add Attribute
        </button>
      </form>
    </div>
  );
};

const AddAttribute: React.FC = () => {
  let { id } = useParams();

  return <CreateAttribute mediaId={id!} />;
};

export default AddAttribute;
