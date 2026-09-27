import { useState, useRef, useEffect } from "react";
import { Document, Page, pdfjs } from "react-pdf";
import "react-pdf/dist/Page/AnnotationLayer.css";
import "react-pdf/dist/Page/TextLayer.css";

pdfjs.GlobalWorkerOptions.workerSrc = `https://cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjs.version}/pdf.worker.min.mjs`;

interface PdfViewerProps {
    url: string;
}

export default function PdfViewer({ url }: PdfViewerProps) {
    const [numPages, setNumPages] = useState<number | null>(null);
    const [pageNumber, setPageNumber] = useState(1);
    const [containerWidth, setContainerWidth] = useState(600);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function updateWidth() {
            if (containerRef.current) {
                setContainerWidth(containerRef.current.clientWidth);
            }
        }
        updateWidth();
        window.addEventListener("resize", updateWidth);
        return () => window.removeEventListener("resize", updateWidth);
    }, []);

    function handleLoadSuccess({ numPages }: { numPages: number }) {
        setNumPages(numPages);
        setPageNumber(1);
    }

    return (
        <div className="pdf-viewer" ref={containerRef}>
            <Document
                file={url}
                onLoadSuccess={handleLoadSuccess}
                loading={<p>PDF yükleniyor...</p>}
                error={<p>PDF yüklenemedi. İndirme linkini deneyebilirsiniz.</p>}
            >
                <Page pageNumber={pageNumber} width={Math.min(containerWidth, 700)} />
            </Document>

            {numPages && (
                <div className="pdf-viewer__controls">
                    <button
                        type="button"
                        className="btn-icon"
                        onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                        disabled={pageNumber <= 1}
                        aria-label="Önceki sayfa"
                    >
                        ◀
                    </button>

                    <div className="pdf-page-selector">
                        <select
                            value={pageNumber}
                            onChange={(e) => setPageNumber(Number(e.target.value))}
                            aria-label="Sayfa seç"
                        >
                            {numPages &&
                                Array.from({ length: numPages }, (_, index) => (
                                    <option key={index + 1} value={index + 1}>
                                        {index + 1}
                                    </option>
                                ))}
                        </select>

                        <span>/ {numPages}</span>
                    </div>

                    <button
                        type="button"
                        className="btn-icon"
                        onClick={() => setPageNumber((p) => Math.min(numPages!, p + 1))}
                        disabled={pageNumber >= numPages!}
                        aria-label="Sonraki sayfa"
                    >
                        ▶
                    </button>
                </div>
            )}
        </div>
    );
}