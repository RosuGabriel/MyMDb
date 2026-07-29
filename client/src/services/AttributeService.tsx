import { Attribute, apiClient } from "../Data";
import { generateUniqueFileName } from "./MediaService";

const API_URL = "media/";

export const addAttribute = async (
  newAttribute: Partial<Attribute>,
  file: File | null,
): Promise<Attribute> => {
  const formData = new FormData();
  formData.append("mediaId", newAttribute.mediaId || "");
  formData.append("type", newAttribute.type || "");
  formData.append("language", newAttribute.language || "");
  if (file) {
    const fileName = await generateUniqueFileName(file.name, file);
    formData.append("file", file, fileName);
  }

  formData.forEach((value, key) => {
    console.log(`${key}: ${value}`);
  });
  console.log(file?.name);

  try {
    const response = await apiClient.post<Attribute>(
      API_URL + "add_attribute",
      formData,
    );
    return response.data;
  } catch (error) {
    console.error("Error adding attribute:", error);
    throw error;
  }
};

export const updateAttribute = async (
  attribute: Partial<Attribute>,
  file: File | null,
): Promise<Attribute> => {
  const formData = new FormData();
  formData.append("mediaId", attribute.mediaId || "");
  formData.append("type", attribute.type || "");
  formData.append("language", attribute.language || "");
  if (file) {
    const fileName = await generateUniqueFileName(file.name, file);
    formData.append("file", file, fileName);
  }

  try {
    const response = await apiClient.put<Attribute>(
      API_URL + "update_attribute",
      formData,
    );
    return response.data;
  } catch (error) {
    console.error("Error updating attribute:", error);
    throw error;
  }
};

export const deleteAttribute = async (
  mediaId: string,
  attributeType: string,
  language: string,
): Promise<void> => {
  try {
    await apiClient.delete(
      API_URL +
        `delete_attribute/${mediaId}?attributeType=${encodeURIComponent(attributeType)}&language=${encodeURIComponent(language)}`,
    );
  } catch (error) {
    console.error("Error deleting attribute:", error);
    throw error;
  }
};
