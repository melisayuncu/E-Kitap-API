import { useState, useEffect } from "react";
import BookUploadForm from "./components/BookUploadForm";
import { createBook, generateBook } from "./services/api";
import type { BookStatus } from "./types/book";
import "./App.css";
import PdfViewer from "./components/PdfViewer";

type AppState = "idle" | "submitting" | "completed" | "error";

interface FunFact {
    text: string;
    url: string;
}

const FUN_FACTS: FunFact[] = [
    {
        text: "Deniz kaplumbağalarının cinsiyetini kumun sıcaklığı belirliyor.",
        url: "https://bilimgenc.tubitak.gov.tr/bunu-biliyor-muydunuz/deniz-kaplumbagalarinin-cinsiyetini-kumun-sicakligi-belirliyor",
    },
    {
        text: "Su samurları, hayvanlar aleminin en yoğun kürke sahip canlılarıdır.",
        url: "https://bilimgenc.tubitak.gov.tr/bunu-biliyor-muydunuz/su-samurlari-hayvanlar-aleminin-en-yogun-kurke-sahip-canlilaridir",
    },
    {
        text: "Bazı canlıların kanları mavi renktedir.",
        url: "https://bilimgenc.tubitak.gov.tr/bunu-biliyor-muydunuz/bazi-canlilarin-kanlari-mavi-renktedir",
    },
    {
        text: "Venedik her yıl 1 ila 2 mm arası bir miktarda batıyor.",
        url: "https://bilimgenc.tubitak.gov.tr/bunu-biliyor-muydunuz/venedik-her-yil-1-ila-2-mm-arasi-bir-miktarda-batiyor",
    },
    {
        text: "Rafflesia arnoldii, dünyanın en büyük çiçeğine sahip bitkisidir.",
        url: "https://bilimgenc.tubitak.gov.tr/bunu-biliyor-muydunuz/rafflesia-arnoldii-dunyanin-en-buyuk-cicegine-sahip-bitkisidir",
    },
    {
        text: "Kar beyaz görünse de aslında yarı saydamdır.",
        url: "https://bilimgenc.tubitak.gov.tr/bunu-biliyor-muydunuz/kar-beyaz-gorunse-de-aslinda-yari-saydamdir",
    },
    {
        text: "Balçık yılan balıkları, dünyanın en fazla mukus salgılayan hayvanlarıdır.",
        url: "https://bilimgenc.tubitak.gov.tr/bunu-biliyor-muydunuz/balcik-yilan-baliklari-dunyanin-en-fazla-mukus-salgilayan-hayvanlaridir",
    },
    {
        text: "Bebek baykuşlar yüz üstü uyur.",
        url: "https://bilimgenc.tubitak.gov.tr/bunu-biliyor-muydunuz/bebek-baykuslar-yuz-ustu-uyur",
    },
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
    }, 5000);
    return () => clearInterval(interval);
  }, [state]);

    async function handleSubmit(bookName: string, files: File[]) {
        setState("submitting");
        setErrorMessage(null);
        setFactIndex(0);

        const MIN_LOADING_MS = 6000;
        const startedAt = Date.now();

        async function waitForMinimumDuration() {
            const elapsed = Date.now() - startedAt;
            const remaining = MIN_LOADING_MS - elapsed;
            if (remaining > 0) {
                await new Promise((resolve) => setTimeout(resolve, remaining));
            }
        }

        try {
            const created = await createBook(bookName, files);
            const generated = await generateBook(created.id);

            await waitForMinimumDuration();

            setBookStatus(generated.status);

            if (generated.status === "Completed" && generated.pdfUrl) {
                setPdfUrl(`https://localhost:7120${generated.pdfUrl}`);
                setState("completed");
            } else {
                setErrorMessage("Kitap oluşturulamadı, durum: " + generated.status);
                setState("error");
            }
        } catch (err) {
            await waitForMinimumDuration();

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
        <div className="page">
            <header className="site-header">
                <div className="site-header__inner">
                    <h1>E-Kitap Oluşturucu</h1>
                </div>
            </header>
            <div className="site-header__accent-bar" />

            <main className="page__content">
                {state === "idle" && (
                    <div className="card">
                        <BookUploadForm onSubmit={handleSubmit} isSubmitting={false} />
                    </div>
                )}

                {state === "submitting" && (
                  <div className="card status-panel">
                    <div className="spinner" />
                    <p>Kitabınız oluşturuluyor...</p>
                    <p className="status-panel__fact">
                      Biliyor muydunuz? {FUN_FACTS[factIndex].text}
                                    <br />
                      <a
              
                        href={FUN_FACTS[factIndex].url}
                        target="_blank"
                        rel="noreferrer"
                        className="status-panel__source"
                      >
                        Kaynak: TÜBİTAK Bilim Genç
                      </a>
                    </p>
                  </div>
                )}

                {state === "error" && (
                    <div className="card">
                        <div className="error-banner" role="alert">
                            {errorMessage}
                            {bookStatus && <div>Durum: {bookStatus}</div>}
                        </div>
                        <button className="btn-secondary" onClick={handleReset}>
                            Tekrar Dene
                        </button>
                    </div>
                )}

                {state === "completed" && pdfUrl && (
          <div className="card result-panel">
            <p className="result-panel__title">E-kitabınız hazır!</p>
            <PdfViewer url={pdfUrl} />
            <a
              className="result-panel__link"
              href={pdfUrl}
              target="_blank"
              rel="noreferrer"
            >
              PDF'i Yeni Sekmede Aç / İndir
            </a>
            <div className="result-panel__actions">
              <button className="btn-secondary" onClick={handleReset}>
                Yeni Kitap Oluştur
              </button>
            </div>
          </div>
    )
}
               
      </main >
    </div >
  );
}

export default App;