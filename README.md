# Aplikasi Web Konversi Excel ke XML (SKK Migas - SOT Production & SOT L-PUMA)

Aplikasi web satu halaman (Single Page Application) untuk konversi data operasional harian industri hulu migas dari format spreadsheet Excel (`.xlsx` / `.xls`) ke standar format XML SKK Migas (ProdML & Loss Production Opportunity), dilengkapi penyimpanan histori harian dan docker containerization.

---

## 🌟 Fitur Utama

1. **Konversi Excel ke XML Terpadu**:
   - Mendukung skema **1. SOT Production (ProdML)** (31 kolom pemetaan sesuai Attachment 2).
   - Mendukung skema **2. SOT L-PUMA** (22 kolom pemetaan sesuai Attachment 3).
   - Drag & drop file Excel (dukungan `.xlsx`, `.xls`, batas ukuran hingga 10 MB).
   - Input nama file kustom (opsional) untuk mempermudah identifikasi di histori.
   - Preview XML langsung dengan syntax viewer dan tombol salin / unduh.

2. **Riwayat Konversi Harian**:
   - Filter pencarian riwayat berdasarkan tanggal konversi.
   - Informasi lengkap: No, Tanggal & Waktu, Nama File, Tipe Skema, Status (`Berhasil` / `Gagal`), Ukuran File, dan Aksi.
   - Tombol langsung untuk mengunduh hasil XML atau membuka modal **Lihat Detail**.
   - Paginasi data riwayat konversi.

3. **Halaman Bantuan & Template Resmi**:
   - Unduh template Excel contoh langsung dari aplikasi:
     - `Template_SOT_Production.xlsx`
     - `Template_SOT_LPUMA.xlsx`
   - Panduan langkah penggunaan 1-4.
   - Tabel referensi lengkap pemetaan kolom Excel ke tag XML.

4. **Containerization dengan Docker Compose**:
   - Backend: .NET 8 Web API + SQLite (Entity Framework Core).
   - Frontend: React 18 + Vite + TypeScript + Nginx reverse proxy.

---

## 🚀 Cara Menjalankan Aplikasi

Pastikan Docker & Docker Compose sudah terpasang di komputer Anda.

### 1. Menjalankan via Docker Compose
Jalankan perintah berikut di direktori root project:
```bash
docker compose up -d
```

### 2. Mengakses Aplikasi
- **Aplikasi Web (Frontend React)**: [http://localhost:3000](http://localhost:3000)
- **Backend API & Dokumentasi Swagger**: [http://localhost:5050/swagger](http://localhost:5050/swagger)

### 3. Menghentikan Aplikasi
```bash
docker compose down
```

---

## 🧪 Pengujian End-to-End (E2E)

Script pengujian otomatis telah disediakan di `test_e2e.py`:
```bash
python3 test_e2e.py
```

Pengujian ini memverifikasi:
- Frontend melayani aset web dengan benar.
- Download template resmi Excel SOT Production dan SOT L-PUMA.
- Konversi kedua template menjadi XML yang valid sesuai hirarki ProdML dan LPO.
- Penyimpanan dan pengambilan data riwayat di database SQLite.
- Unduhan file XML dari endpoint backend.

---

## 📁 Struktur Direktori

```
.
├── docker-compose.yml              # Konfigurasi multi-container Docker
├── test_e2e.py                     # Script verifikasi otomatis end-to-end
├── backend/
│   └── ExcelToXmlApi/
│       ├── Controllers/            # API Endpoints (ConverterController)
│       ├── Data/                   # DbContext SQLite (AppDbContext)
│       ├── DTOs/                   # Data Transfer Objects
│       ├── Models/                 # Model Data & Pemetaan Kolom
│       ├── Services/               # Converter SOT Production, SOT L-PUMA & Generator Template
│       ├── Dockerfile              # Dockerfile backend .NET 8
│       └── Program.cs              # Konfigurasi aplikasi & dependencies
└── frontend/
    ├── src/
    │   ├── components/             # Header, Sidebar, Form, History, Help, Modal
    │   ├── types/                  # Definisi TypeScript
    │   ├── App.tsx                 # Main SPA Layout
    │   ├── index.css               # Desain UI modern korporat
    │   └── main.tsx                # Entry point React
    ├── nginx.conf                  # Nginx proxy & SPA router
    ├── Dockerfile                  # Dockerfile frontend
    └── package.json
```
