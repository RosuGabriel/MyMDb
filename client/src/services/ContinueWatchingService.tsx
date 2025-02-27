import axios from "axios";
import { apiClient, IContinueWatching } from "../Data";
import { MEDIA_URL } from "./MediaService";

export const fetchContinueWatching = async (): Promise<IContinueWatching[]> => {
  try {
    const response = await apiClient.get<{ $values: IContinueWatching[] }>(
      MEDIA_URL + "continue_watching"
    );
    return response.data.$values;
  } catch (error) {
    console.error("Error fetching continue watching:", error);
    throw error;
  }
};

export const fetchContinueWatchingById = async (
  mediaId: string,
  episodeId: string | undefined
): Promise<IContinueWatching> => {
  try {
    if (!episodeId) {
      const response = await apiClient.get<IContinueWatching>(
        MEDIA_URL + "continue_watching/" + mediaId
      );
      return response.data;
    } else {
      const response = await apiClient.get<IContinueWatching>(
        MEDIA_URL + "continue_watching/" + mediaId + "/" + episodeId
      );
      return response.data;
    }
  } catch (error) {
    console.error("Error fetching continue watching:", error);
    throw error;
  }
};

export const addContinueWatching = async (
  continueWatching: Partial<IContinueWatching>
): Promise<IContinueWatching> => {
  try {
    const response = await apiClient.post<IContinueWatching>(
      MEDIA_URL + "continue_watching",
      continueWatching
    );
    return response.data;
  } catch (error) {
    console.error("Error adding continue watching:", error);
    throw error;
  }
};

export const deleteContinueWatching = async (
  continueWatching: Partial<IContinueWatching>
): Promise<void> => {
  try {
    const response = await apiClient.delete<IContinueWatching>(
      MEDIA_URL + "continue_watching",
      { data: continueWatching }
    );
    return;
  } catch (error) {
    console.error("Error deleting continue watching:", error);
    throw error;
  }
};
