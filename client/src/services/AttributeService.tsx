import { Attribute, axiosInstance } from "../Data";

const API_URL = "media/";

export const addAttribute = async (
  newAttribute: Partial<Attribute>,
  file: File | null
): Promise<Attribute> => {
  const formData = new FormData();
  formData.append("mediaId", newAttribute.mediaId || "");
  formData.append("type", newAttribute.type || "");
  formData.append("language", newAttribute.language || "");
  if (file) {
    formData.append("file", file);
  }

  formData.forEach((value, key) => {
    console.log(`${key}: ${value}`);
  });
  console.log(file?.name);

  try {
    const response = await axiosInstance.post<Attribute>(
      API_URL + "add_attribute",
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error adding attribute:", error);
    throw error;
  }
};
