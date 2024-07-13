import { Media, axiosInstance } from "../Data";

const MEDIA_URL = "media/";

export const fetchMedia = async (): Promise<Media[]> => {
  try {
    const response = await axiosInstance.get<{ $values: Media[] }>(
      MEDIA_URL + "movies_and_series"
    );
    return response.data.$values;
  } catch (error) {
    console.error("Error fetching media:", error);
    throw error;
  }
};

export const fetchMovies = async (): Promise<Media[]> => {
  try {
    const response = await axiosInstance.get<{ $values: Media[] }>(
      MEDIA_URL + "movies"
    );
    return response.data.$values;
  } catch (error) {
    console.error("Error fetching movies:", error);
    throw error;
  }
};

export const fetchSeries = async (): Promise<Media[]> => {
  try {
    const response = await axiosInstance.get<{ $values: Media[] }>(
      MEDIA_URL + "series"
    );
    return response.data.$values;
  } catch (error) {
    console.error("Error fetching series:", error);
    throw error;
  }
};

export const fetchMediaById = async (id: string): Promise<Media> => {
  const response = await axiosInstance.get(MEDIA_URL + id);
  return response.data;
};

export const createMovie = async (
  newMovie: Partial<Media>,
  poster: File | null,
  video: File | null
): Promise<Media> => {
  const formData = new FormData();
  formData.append("title", newMovie.title || "");
  formData.append("description", newMovie.description || "");
  formData.append("releaseDate", newMovie.releaseDate?.toISOString() || "");

  if (poster) {
    const uniqueImageName = await generateUniqueFileName(
      newMovie.title || "N/A" + Date.now().toString(),
      poster
    );
    formData.append("poster", poster, uniqueImageName);
    formData.append("posterPath", uniqueImageName);
  }

  if (video) {
    const uniqueVideoName = await generateUniqueFileName(
      newMovie.title || "N/A",
      video
    );
    formData.append("video", video, uniqueVideoName);
    formData.append("videoPath", uniqueVideoName);
  }

  try {
    const response = await axiosInstance.post<Media>(
      MEDIA_URL + "add_movie",
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
      }
    );

    return response.data;
  } catch (error) {
    console.error("Error creating movie:", error);
    throw error;
  }
};

export const deleteMedia = async (id: string): Promise<void> => {
  try {
    await axiosInstance.delete(MEDIA_URL + "delete_media/" + id);
  } catch (error) {
    console.error("Error deleting media:", error);
    throw error;
  }
};

const generateUniqueFileName = async (
  title: string,
  file: File
): Promise<string> => {
  let extension = getFileExtension(file.name);
  if (extension == "mkv") {
    extension = "mp4";
  }
  //const baseName = file.name.replace(`.${extension}`, "");
  //const timestamp = Date.now();
  const uniqueFileName = `${title}.${extension}`;
  return uniqueFileName;
};

const getFileExtension = (fileName: string): string => {
  return fileName.split(".").pop() || "";
};
