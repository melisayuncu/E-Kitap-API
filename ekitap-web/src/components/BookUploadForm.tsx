import { useState, useRef } from "react";

interface BookUploadFormProps {
  onSubmit: (bookName: string, files: File[]) => void;
  isSubmitting: boolean;
}

export default function BookUploadForm({ onSubmit, isSubmitting }: BookUploadFormProps) {
  const [bookName, setBookName] = useState("");
  const [files, setFiles] = useState<File[]>([]);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const REQUIRED_FILE_COUNT = 10;

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    setError(null);
    const selected = Array.from(e.target.files ?? []);

    const nonDocx = selected.filter(
      (f) => !f.name.toLowerCase().endsWith(".docx")
    );
    if (nonDocx.length > 0) {
      setError("Yalnızca .docx dosyaları yükleyebilirsiniz.");
      if (fileInputRef.current) fileInputRef.current.value = "";
      return;
    }

    const combined = [...files, ...selected];

    if (combined.length > REQUIRED_FILE_COUNT) {
      setError(
        `En fazla ${REQUIRED_FILE_COUNT} dosya seçebilirsiniz. Şu an ${files.length} dosya seçili, ${selected.length} dosya daha eklemeye çalıştınız.`
      );
      if (fileInputRef.current) fileInputRef.current.value = "";
      return;
    }

    setFiles(combined);
    if (fileInputRef.current) fileInputRef.current.value = "";
  }

  function handleRemoveFile(index: number) {
    setError(null);
    setFiles((prev) => prev.filter((_, i) => i !== index));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (bookName.trim() === "") {
      setError("Lütfen kitap adını girin.");
      return;
    }
    if (files.length !== REQUIRED_FILE_COUNT) {
      setError(`Tam olarak ${REQUIRED_FILE_COUNT} dosya seçmelisiniz.`);
      return;
    }
    onSubmit(bookName, files);
  }

  const isComplete = files.length === REQUIRED_FILE_COUNT;

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label htmlFor="bookName">Kitap Adı</label>
        <input
          id="bookName"
          type="text"
          value={bookName}
          onChange={(e) => setBookName(e.target.value)}
          disabled={isSubmitting}
          placeholder="Örn: 2026 Konferansı Bildiri Kitabı"
        />
      </div>

      <div>
        <label htmlFor="fileInput">
          Bildiri Dosyaları ({files.length}/{REQUIRED_FILE_COUNT})
        </label>
        <input
          id="fileInput"
          ref={fileInputRef}
          type="file"
          accept=".docx"
          multiple
          onChange={handleFileChange}
          disabled={isComplete || isSubmitting}
        />
      </div>

      {error && <p role="alert">{error}</p>}

      {files.length > 0 && (
        <ol>
          {files.map((file, index) => (
            <li key={`${file.name}-${index}`}>
              {index + 1}. {file.name}
              <button
                type="button"
                onClick={() => handleRemoveFile(index)}
                disabled={isSubmitting}
              >
                Kaldır
              </button>
            </li>
          ))}
        </ol>
      )}

      <button type="submit" disabled={!isComplete || bookName.trim() === "" || isSubmitting}>
        {isSubmitting ? "Kitap Oluşturuluyor..." : "Kitabı Oluştur"}
      </button>
    </form>
  );
}