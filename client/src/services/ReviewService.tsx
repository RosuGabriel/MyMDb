import { Review, apiClient } from "../Data";

const API_URL = "user/";

export const addReview = async (
  newReview: Partial<Review>
): Promise<Review> => {
  try {
    const response = await apiClient.post<Review>(
      API_URL + "add_review",
      newReview
    );
    return await response.data;
  } catch (error) {
    console.error("Error adding review:", error);
    throw error;
  }
};

export const deleteReview = async (mediaId: string): Promise<void> => {
  try {
    const response = await apiClient.delete(
      API_URL + "delete_review/" + mediaId
    );
    return response.data;
  } catch (error) {
    console.error("Error deleting review:", error);
    throw error;
  }
};
