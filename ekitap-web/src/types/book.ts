
//We define the structure of the JSON responses from the backend here

export type BookStatus = "Pending" | "Processing" | "Completed" | "Failed";

export interface CreateBookResponse {
    id: number;
    name: string;
    status: BookStatus;
    submissionFileNames: string[];
}

export interface GenerateBookResponse {
    id: number;
    name: string;
    status: BookStatus;
    pdfUrl?: string;
}