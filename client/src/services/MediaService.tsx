import { Media, apiClient } from "../Data";

export const MEDIA_URL = "media/";

export const fetchMedia = async (): Promise<Media[]> => {
  try {
    const response = await apiClient.get<{ $values: Media[] }>(
      MEDIA_URL + "movies_and_series",
    );
    return response.data.$values;
  } catch (error) {
    console.error("Error fetching media:", error);
    throw error;
  }
};

export const fetchMovies = async (): Promise<Media[]> => {
  try {
    const response = await apiClient.get<{ $values: Media[] }>(
      MEDIA_URL + "movies",
    );
    return response.data.$values;
  } catch (error) {
    console.error("Error fetching movies:", error);
    throw error;
  }
};

export const fetchSeries = async (): Promise<Media[]> => {
  try {
    const response = await apiClient.get<{ $values: Media[] }>(
      MEDIA_URL + "series",
    );
    return response.data.$values;
  } catch (error) {
    console.error("Error fetching series:", error);
    throw error;
  }
};

export const fetchMediaById = async (id: string): Promise<Media> => {
  const response = await apiClient.get(MEDIA_URL + id);
  return response.data;
};

export const createMovie = async (
  newMovie: Partial<Media>,
  poster: File | null,
  video: File | null,
  onUploadProgress: (progressEvent: any) => void,
): Promise<Media> => {
  const formData = new FormData();
  formData.append("title", newMovie.title || "");
  formData.append("description", newMovie.description || "");
  formData.append("releaseDate", newMovie.releaseDate?.toString() || "");

  if (poster) {
    const uniqueImageName = await generateUniqueFileName(
      newMovie.title || "N/A" + Date.now().toString(),
      poster,
    );
    formData.append("poster", poster, uniqueImageName);
    formData.append("posterPath", uniqueImageName);
  }

  if (video) {
    const uniqueVideoName = await generateUniqueFileName(
      newMovie.title || "N/A",
      video,
    );
    formData.append("video", video, uniqueVideoName);
    formData.append("videoPath", uniqueVideoName);
  }

  try {
    const response = await apiClient.post<Media>(
      MEDIA_URL + "add_movie",
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress,
      },
    );

    return response.data;
  } catch (error) {
    console.error("Error creating movie:", error);
    throw error;
  }
};

export const createSeries = async (
  newSeries: Partial<Media>,
  poster: File | null,
  onUploadProgress: (progressEvent: any) => void,
): Promise<Media> => {
  const formData = new FormData();
  formData.append("title", newSeries.title || "");
  formData.append("description", newSeries.description || "");
  formData.append("releaseDate", newSeries.releaseDate?.toString() || "");
  formData.append("seasons", newSeries.seasons?.toString() || "1");

  if (poster) {
    const uniqueImageName = await generateUniqueFileName(
      newSeries.title || "N/A" + Date.now().toString(),
      poster,
    );
    formData.append("poster", poster, uniqueImageName);
    formData.append("posterPath", uniqueImageName);
  }

  try {
    const response = await apiClient.post<Media>(
      MEDIA_URL + "add_series",
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress,
      },
    );

    return response.data;
  } catch (error) {
    console.error("Error creating series:", error);
    throw error;
  }
};

export const createEpisode = async (
  newEpisode: Partial<Media>,
  poster: File | null,
  video: File | null,
  onUploadProgress: (progressEvent: any) => void,
): Promise<Media> => {
  const formData = new FormData();
  formData.append("seriesId", newEpisode.id || "");
  formData.append("title", newEpisode.title || "");
  formData.append("description", newEpisode.description || "");
  formData.append("releaseDate", newEpisode.releaseDate?.toString() || "");
  formData.append("seasonNumber", newEpisode.seasonNumber?.toString() || "");
  formData.append("episodeNumber", newEpisode.episodeNumber?.toString() || "");

  if (poster) {
    const imageName = `${newEpisode.seasonNumber?.toString() || ""}-${
      newEpisode.episodeNumber?.toString() || ""
    }.${getFileExtension(poster.name)}`;
    formData.append("poster", poster, imageName);
    formData.append("posterPath", imageName);
  }

  if (video) {
    const videoName = await generateUniqueFileName(
      `S${newEpisode.seasonNumber?.toString() || ""}-E${
        newEpisode.episodeNumber?.toString() || ""
      }`,
      video,
    );
    formData.append("video", video, videoName);
    formData.append("videoPath", videoName);
  }

  try {
    const response = await apiClient.post<Media>(
      MEDIA_URL + "add_episode",
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress,
      },
    );

    return response.data;
  } catch (error) {
    console.error("Error creating episode:", error);
    throw error;
  }
};

export const deleteMedia = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(MEDIA_URL + "delete_media/" + id);
  } catch (error) {
    console.error("Error deleting media:", error);
    throw error;
  }
};

export const editMovie = async (
  id: string,
  movieToEdit: Partial<Media>,
  poster: File | null,
  video: File | null,
  onUploadProgress: (progressEvent: any) => void,
): Promise<Media> => {
  const formData = new FormData();
  formData.append("id", id);
  formData.append("title", movieToEdit.title || "");
  formData.append("description", movieToEdit.description || "");
  formData.append("releaseDate", movieToEdit.releaseDate?.toString() || "");

  if (poster) {
    const uniqueImageName = await generateUniqueFileName(
      movieToEdit.title || "N/A" + Date.now().toString(),
      poster,
    );
    formData.append("poster", poster, uniqueImageName);
    formData.append("posterPath", uniqueImageName);
  } else {
    // Send existing path so it can be edited without uploading a new file
    formData.append("posterPath", movieToEdit.posterPath || "");
  }

  if (video) {
    const uniqueVideoName = await generateUniqueFileName(
      movieToEdit.title || "N/A",
      video,
    );
    formData.append("video", video, uniqueVideoName);
    formData.append("videoPath", uniqueVideoName);
  } else {
    // Send existing path so it can be edited without uploading a new file
    formData.append("videoPath", movieToEdit.videoPath || "");
  }

  try {
    const response = await apiClient.post<Media>(
      MEDIA_URL + "edit_movie/" + id,
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress,
      },
    );

    return response.data;
  } catch (error) {
    console.error("Error editing movie:", error);
    throw error;
  }
};

export const editSeries = async (
  id: string,
  seriesToEdit: Partial<Media>,
  poster: File | null,
  onUploadProgress: (progressEvent: any) => void,
): Promise<Media> => {
  const formData = new FormData();
  formData.append("id", id);
  formData.append("title", seriesToEdit.title || "");
  formData.append("description", seriesToEdit.description || "");
  formData.append("releaseDate", seriesToEdit.releaseDate?.toString() || "");
  formData.append("seasons", seriesToEdit.seasons?.toString() || "1");

  if (poster) {
    const uniqueImageName = await generateUniqueFileName(
      seriesToEdit.title || "N/A" + Date.now().toString(),
      poster,
    );
    formData.append("poster", poster, uniqueImageName);
    formData.append("posterPath", uniqueImageName);
  } else {
    // Send existing path so it can be edited without uploading a new file
    formData.append("posterPath", seriesToEdit.posterPath || "");
  }

  try {
    const response = await apiClient.post<Media>(
      MEDIA_URL + "edit_series/" + id,
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress,
      },
    );

    return response.data;
  } catch (error) {
    console.error("Error editing series:", error);
    throw error;
  }
};

export const editEpisode = async (
  id: string,
  episodeToEdit: Partial<Media>,
  poster: File | null,
  video: File | null,
  onUploadProgress: (progressEvent: any) => void,
): Promise<Media> => {
  const formData = new FormData();
  formData.append("id", id);
  formData.append("seriesId", episodeToEdit.seriesId?.toString() || "");
  formData.append("title", episodeToEdit.title || "");
  formData.append("description", episodeToEdit.description || "");
  formData.append("releaseDate", episodeToEdit.releaseDate?.toString() || "");
  formData.append("seasonNumber", episodeToEdit.seasonNumber?.toString() || "");
  formData.append(
    "episodeNumber",
    episodeToEdit.episodeNumber?.toString() || "",
  );

  if (poster) {
    const imageName = `${episodeToEdit.seasonNumber?.toString() || ""}-${
      episodeToEdit.episodeNumber?.toString() || ""
    }.${getFileExtension(poster.name)}`;
    formData.append("poster", poster, imageName);
    formData.append("posterPath", imageName);
  } else {
    // Send existing path so it can be edited without uploading a new file
    formData.append("posterPath", episodeToEdit.posterPath || "");
  }

  if (video) {
    const videoName = await generateUniqueFileName(
      `S${episodeToEdit.seasonNumber?.toString() || ""}-E${
        episodeToEdit.episodeNumber?.toString() || ""
      }`,
      video,
    );
    formData.append("video", video, videoName);
    formData.append("videoPath", videoName);
  } else {
    // Send existing path so it can be edited without uploading a new file
    formData.append("videoPath", episodeToEdit.videoPath || "");
  }

  try {
    const response = await apiClient.post<Media>(
      MEDIA_URL + "edit_episode/" + id,
      formData,
      {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress,
      },
    );

    return response.data;
  } catch (error) {
    console.error("Error editing episode:", error);
    throw error;
  }
};

export const generateUniqueFileName = async (
  title: string,
  file: File,
): Promise<string> => {
  let extension = getFileExtension(file.name);
  if (extension == "mkv") {
    extension = "mp4";
  }
  if (extension == "srt") {
    extension = "vtt";
  }
  const uniqueFileName = `${title}.${extension}`;
  return uniqueFileName;
};

export const getFileExtension = (fileName: string): string => {
  let extension = fileName.split(".").pop() || "";
  return extension;
};
