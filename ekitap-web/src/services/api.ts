import axios from "axios";
import type { CreateBookResponse, GenerateBookResponse } from "../types/book";

const API_BASE_URL = "https://localhost:7120/api";

export async function createBook(
    bookName: string,
    files: File[]
): Promise<CreateBookResponse> {
    const formData = new FormData();
    formData.append("bookName", bookName);
    files.forEach((file) => {
        formData.append("files", file);
    });

    const response = await axios.post<CreateBookResponse>(
        `${API_BASE_URL}/Books`,
        formData,
        {
            headers: { "Content-Type": "multipart/form-data" },
        }
    );

    return response.data;
}

export async function generateBook(bookId: number): Promise<GenerateBookResponse> {
    const response = await axios.post<GenerateBookResponse>(
        `${API_BASE_URL}/Books/${bookId}/generate`
    );

    return response.data;
}