# Bildirilerden E-Kitap Oluşturma 

10 ayrı Word (.docx) bildirisini sisteme yükleyerek, tek bir PDF e-kitap üretme çalışması.

## Kurulum

### Gereksinimler
- .NET 8 SDK
- Node.js 
- MSSQL (‘SQLEXPRESS’)

### Backend

1. `E-Kitap-API` klasörünü Visual Studio ile açın.
2. `appsettings.json` içindeki bağlantı dizesini kendi SQL Server instance adına göre düzenleyin:
```json
   "ConnectionStrings": {
     "Default": "Server=localhost\\SQLEXPRESS;Database=EkitapDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
```
3. Package Manager Console açın ve migration uygulayın
Update-Database
   Bu sayede `EkitapDb` veritabanınız ve `Books` / `Submissions` tabloları otomatik oluşturulur.
4. F5 ile çalıştırın. API `https://localhost:7120` adresinde açılır (port farklıysa `ekitap-web/src/services/api.ts` içindeki `API_BASE_URL`'i güncelleyebilirsiniz).

### Frontend

1. `ekitap-web` klasörünü Visual Studio’da veya terminalde açın.
2. Paketleri kurun:
3. npm install
4. npm run dev
5. Tarayıcıda `http://localhost:5173` adresini açın.

**Not:** Backend'in CORS ayarı `http://localhost:5173` origin'ine izin verecek şekilde yapılandırılmıştır (`Program.cs`). Frontend farklı bir portta çalışırsa backend'deki CORS policy'sinin de güncellenmesi gerekir.

## Veri Modeli

İki ana tablo kullanıldı: **Books** ve **Submissions** yani **Kitaplar** ve **Bildiriler**
**Books** tablosu, her “Kitabı Oluştur” isteği karşılığında 1 kayıt oluşturur.
-kitap adı
-oluşturma tarihi
-durum (pending/processing/completed/failed)
-üretilen pdf’in yolu
-hata mesajı
**Submissions** tablosu ise her yüklenen .docx için bir kayıt oluşturur:
-başlık
-dosya adı
-orijinal dosya yolu
-sıra numarası
-üretilen pdf’teki başlangıç sayfası

`Submissions.BookId`, `Books.Id`'ye foreign key ile bağlı. Kod tarafında İngilizce isimlendirme tercih ettim (Books/Submissions), ancak kavramsal karşılığı doğrudan Kitaplar/Bildiriler'dir.


## Dosya Saklama Yaklaşımı

Yüklenen orijinal docx dosyaları `wwwroot/uploads/{kitapId}/` altında, dosya adının başına yükleme sırasını belirten bir önek (`01_`, `02_`, ...) eklenerek saklanıyor. Üretilen PDF'ler `wwwroot/generated/kitap_{kitapId}.pdf` olarak kaydedilip `app.UseStaticFiles()` ile doğrudan URL üzerinden (`/generated/kitap_{id}.pdf`) erişilebilir/indirilebilir hale getiriliyor. 

## Kullanıcı Akışı

1. Kullanıcı kitap adı girer ve 10 adet `.docx` seçer: Burada frontend, 10'dan az veya fazla seçime izin vermez.
2. `POST /api/Books` — dosyalar sunucuya kaydedilir, her biri için Word içeriği okunup gerçek başlık (docx içindeki "Title" stilinden veya ilk paragraftan) çıkarılır, `Books` ve `Submissions` kayıtları oluşturulur (durum: `Pending`).
3. `POST /api/Books/{id}/generate` — her bildiri tekrar okunur, iletişim bilgisi temizlenir, her biri tek başına sanal olarak PDF'e basılıp kaç sayfa tuttuğu ölçülür, bu ölçümlerden içindekiler sayfası için başlangıç sayfaları hesaplanır, son olarak tüm kitap (içindekiler ve 10 bildiri, sayfa numaralarıyla) tek PDF olarak üretilir. Durum `Completed` yapılır. Herhangi bir adımda hata olursa durum `Failed` yapılır ve hata mesajı kaydedilir.
4. Frontend, PDF’i sayfa içerisinde sunar, sayfalar arası geçiş yapılabilir. Ayrıca PDF'i tarayıcının kendi görüntüleyicisinde açılabilecek/indirilebilecek bir link olarak da sunar.

Bildiriler, yükleme sırasına göre birleştirilir. Kullanıcı ayrıca isterse arayüzde ok butonlarıyla sırayı değiştirebilir.

## E-posta Adresi ve Telefon Numarası Temizliği

PDF üretimi sırasında (orijinal docx değiştirilmeden) çok aşamalı bir regex işlemiyle temizlik yapılır:

1. `ORCID` etiketinden sonra gelen numara geçici olarak işaretlenip sonraki adımlardan korunur.
2.  Standart e-posta deseni silinir.
3.  Rakam + boşluk/parantez/tire/nokta karışımı diziler bulunur; içindeki gerçek rakam sayısı 10-12 arasındaysa (Türkiye telefon numarası uzunluğu) silinir — bu sayede parantezli, tireli, `+90`'lı farklı formatlar yakalanırken ORCID'in 16 haneli yapısına dokunulmaz.
4. "Tel:", "GSM:", "İletişim:" gibi etiketler de (kelime sınırı + zorunlu kolon şartıyla) temizlenir — bu şart, "nitel" kelimesindeki "tel" veya "İletişim Fakültesi" / "Mobile Applications" gibi gerçek içerik kelimelerinin yanlışlıkla silinmesini engeller.
5. Telefon numarası silindikten sonra satırın en sonunda tek başına kalan etiketler ("E-mail | Mobile" gibi) de temizlenir — ama yalnızca arkalarında gerçek içerik yoksa.
6. ORCID geri yüklenir, geriye kalan dağınık ayraçlar (`- -`, `| |` gibi tekrarlar) temizlenir.


| Orijinal | Temizlenmiş |
|---|---|
| `E-posta: elif.kaya@example.org \| Tel: 0500 000 00 01 \| ORCID: 0000-0001-1000-0001` | `ORCID: 0000-0001-1000-0001` |
| `İletişim: - - ORCID 0000-0001-1000-0003` | `ORCID 0000-0001-1000-0003` |
| `E-mail \| Mobile +90-500-000-00-06` | *(tamamen silindi)* |
| `İç Anadolu Örnek Üniversitesi, İletişim Fakültesi, Ankara` | *(gerçek içerik korunuyor)* |


## Kullanılan Kütüphaneler

**Backend:**
- `DocumentFormat.OpenXml` — Word (.docx) içeriğini okuma
- `QuestPDF` — PDF üretimi 
- `PdfSharpCore` — üretilen ara PDF'lerin sayfa sayısını ölçme (içindekiler hesaplaması için)
- `Microsoft.EntityFrameworkCore.SqlServer` / `.Design` / `.Tools` — MSSQL erişimi ve migration

**Frontend:**
- `axios` — backend API istekleri
- React + TypeScript + Vite
- `react-pdf` - PDF’i sayfada görüntüleyebilmek için

## Tasarım Kararları

- Loading ekranında gerçek yüzde ilerleme yerine dönen bilgi mesajları (TÜBİTAK Bilim Genç kaynaklı, kaynak linkli) gösteriliyor; backend'in gerçek süresi çok kısa olabildiği için ekranın en az 6 saniye görünmesi frontend'de garanti altına alındı. Bu bilgiler, bekleme aşamasında bir genel kültür yarışması gibi, şıklı sorular halinde sunulabilirdi. Fakat PDF oluşturma için bekleme süresi bu kadar uzun olmadığından (süre uzatılabilir fakat bu da artık kullanıcıyı sıkabilir) vazgeçildi.
- Arayüz, dar ekranlarda (telefon/tablet) da rahat kullanılabilmesi için tasarlandı. Dar ekranlarda boşluklar daralıyor, dosya listesi ve butonlar alt alta geçip dokunması kolay büyüklükte gösteriliyor, PDF görüntüleyici de ekran genişliğine göre otomatik küçülüp büyüyor.
- Dosya sıralaması sürükle-bırak yerine yukarı/aşağı ok butonlarıyla yapılıyor — mobilde ve diğer dokunmatik cihazlarda güvenilir çalışması için.
- Site tasarımı https://akap.tr/ web sitesi tasarımından esinlenildi.

## Sınırlamalar

- İçindekiler sayfasının tek sayfaya sığacağı varsayılıyor (10 bildirilik testlerde sorun çıkmadı, ama çok uzun başlıklarla riskli).
- Bildiri içindeki İngilizce başlıklar içindekiler listesine dahil edilmiyor, sadece Türkçe başlık kullanılıyor.

## Yapay Zeka Kullanımı

Özellikle kullanılabilecek kütüphanelerin araştırılması (Word/PDF işlemleri için uygun .NET kütüphanelerinin değerlendirilmesi), wwwroot altında dosya saklama, PDF oluşturma yaklaşımları ve sayfa numarası yönetimi (SubmissionPdfDocument yapısı), iletişim bilgisi temizliğinde regex mantığı, React tarafında PDF görüntüleme ve arayüzde bazı CSS düzenlemeleri gibi belirli teknik konularda yapay zekadan yardım aldım.  
İletişim bilgisi temizliğinde ortaya çıkan hatalı eşleşmeleri gerçek test verileriyle karşılaştırarak yapay zeka ile birlikte düzelttik.



