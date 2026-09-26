import { useState, useEffect } from "react";
import BookUploadForm from "./components/BookUploadForm";
import { createBook, generateBook } from "./services/api";
import type { BookStatus } from "./types/book";

type AppState = "idle" | "submitting" | "completed" | "error";

const FUN_FACTS = [
  "Biliyor muydun? İçindekiler sayfası, her bildirinin gerçek başlangıç sayfasına göre otomatik hesaplanıyor.",
  "E-kitabınız hazırlanırken e-posta ve telefon numaraları da temizleniyor.",
  "10 bildiri tek bir PDF'te, sayfa numaraları korunarak birleştiriliyor.",
  "Neredeyse hazır — Word belgeleriniz PDF'e dönüştürülüyor.",
];

function App() {
  const [state, setState] = useState<AppState>("idle");
  const [factIndex, setFactIndex] = useState(0);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);
  const [bookStatus, setBookStatus] = useState<BookStatus | null>(null);

  useEffect(() => {
    if (state !== "submitting") return;
    const interval = setInterval(() => {
      setFactIndex((prev) => (prev + 1) % FUN_FACTS.length);
    }, 3000);
    return () => clearInterval(interval);
  }, [state]);

  async function handleSubmit(bookName: string, files: File[]) {
    setState("submitting");
    setErrorMessage(null);
    setFactIndex(0);

    try {
      const created = await createBook(bookName, files);
      const generated = await generateBook(created.id);

      setBookStatus(generated.status);

      if (generated.status === "Completed" && generated.pdfUrl) {
        setPdfUrl(`https://localhost:7120${generated.pdfUrl}`);
        setState("completed");
      } else {
        setErrorMessage("Kitap oluşturulamadı, durum: " + generated.status);
        setState("error");
      }
    } catch (err) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data
          ?.message ?? "Beklenmeyen bir hata oluştu, lütfen tekrar deneyin.";
      setErrorMessage(message);
      setState("error");
    }
  }

  function handleReset() {
    setState("idle");
    setErrorMessage(null);
    setPdfUrl(null);
    setBookStatus(null);
  }

  return (
    <div>
      <h1>Bildirilerden E-Kitap Oluşturma</h1>

      {state === "idle" && (
        <BookUploadForm onSubmit={handleSubmit} isSubmitting={false} />
      )}

      {state === "submitting" && (
        <div>
          <p>Kitabınız oluşturuluyor...</p>
          <p>{FUN_FACTS[factIndex]}</p>
        </div>
      )}

      {state === "error" && (
        <div>
          <p role="alert">{errorMessage}</p>
          {bookStatus && <p>Durum: {bookStatus}</p>}
          <button onClick={handleReset}>Tekrar Dene</button>
        </div>
      )}

      {state === "completed" && pdfUrl && (
        <div>
          <p>E-kitabınız hazır!</p>
          <a href={pdfUrl} target="_blank" rel="noreferrer">
            PDF'i yeni sekmede aç / indir
          </a>
          <button onClick={handleReset}>Yeni Kitap Oluştur</button>
        </div>
      )}
    </div>
  );
}

export default App;