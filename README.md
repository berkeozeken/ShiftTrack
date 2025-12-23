# ShiftTrack

ShiftTrack, personel mesai tanýmlarý ve giriþ/çýkýþ verileri üzerinden günlük çalýþma süresi ve fazla mesai hesaplamasý yapan bir ASP.NET Core tabanlý uygulamadýr.

---

## Özellikler

### Mesai Tanýmlarý
- Mesai baþlangýç ve bitiþ saatleri tanýmlanabilir
- Mola baþlangýç ve bitiþ saatleri girilebilir
- Fazla mesai hakký tanýmlanabilir
- Tanýmlý mesailer düzenlenebilir ve silinebilir

### Mesai Hesaplayýcý
- Personel bazlý giriþ / çýkýþ verileri girilebilir
- Ayný personel ve ayný gün için **tek sonuç** üretilir
- Günün **ilk giriþ** ve **son çýkýþ** saatleri esas alýnýr
- Erken gelme fazla mesai olarak deðerlendirilmez
- Mesai bitiþinden sonra çalýþýlan süre fazla mesai olarak hesaplanýr
- Gün bazlý gruplama yapýlýr
- Zaman çakýþmalarýnda kapsayýcý aralýk dikkate alýnýr

---

## Kullanýlan Teknolojiler
- ASP.NET Core Web API
- ASP.NET Core MVC
- Entity Framework Core
- MSSQL Server 2022
- Docker & Docker Compose
- Bootstrap 5

---

## Proje Yapýsý
- ShiftTrack.Api — Web API katmaný
- ShiftTrack.Web — MVC UI katmaný
- db — Veritabaný script ve dump dosyalarý
- docker-compose.yml — MSSQL Docker konfigürasyonu
- .env — Ortam deðiþkenleri (**gitignore**)
- ShiftTrack.sln — Solution dosyasý

---

## Kurulum ve Çalýþtýrma

### Gereksinimler
- .NET SDK
- Docker (Docker Compose dahil)
- Git

### Adýmlar
1. Repository'yi klonlayýn.
2. Proje kök dizininde MSSQL'i Docker ile ayaða kaldýrýn:
   - `docker compose up -d`
3. Ortam deðiþkenlerini `.env` dosyasý üzerinden tanýmlayýn (**.env repoya dahil deðildir**).
4. Visual Studio üzerinden `ShiftTrack.sln` dosyasýný açýn.
5. `ShiftTrack.Web` projesini baþlangýç projesi olarak çalýþtýrýn.

Alternatif olarak:
- `cd ShiftTrack.Web`
- `dotnet run`

---

## Geliþtirici
Berke Özeken  
Software Engineer  
E-posta: berkeozeken1607@gmail.com  
GitHub: https://github.com/berkeozeken  
Linkedin: https://www.linkedin.com/in/berke-ozeken/

