export interface Media {
  id: string;
  createDate: Date;
  modifiedDate: Date;
  title: string;
  description: string;
  releaseDate?: Date;
  posterPath: string;
  videoPath: string;
  episodeNumber: number;
  seasonNumber: number;
  reviews: Review[];
}

export interface Review {
  id: string;
  createDate: Date;
  modifiedDate: Date;
  rating: number;
  comment: string;
}
