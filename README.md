# ChatLedger - Proof of Authority (PoA) Tabanlı Blockchain

ChatLedger, verilerin değişmezliğini ve güvenliğini **Proof of Authority (PoA)** konsensüs mekanizması ile sağlayan, .NET tabanlı ve MongoDB destekli hafif bir blockchain uygulamasıdır.

Geleneksel blockchain'lerdeki (Bitcoin vb.) yüksek enerji tüketen **Mining (Madencilik)** işlemlerine ihtiyaç duymaz. Bunun yerine, güvenilir bir "Otorite" (bu sunucu), verileri kriptografik olarak imzalar ve zincire ekler.

## 🚀 Temel Özellikler

*   **Proof of Authority (PoA)**: Bloklar, sistem tarafından üretilen RSA anahtarları ile imzalanır. Sadece yetkili anahtara sahip olan sunucu veri ekleyebilir.
*   **Değişmezlik (Immutability)**: Her kayıt, bir önceki kaydın kriptografik özetini (Hash) içerir. Geçmişe dönük bir veri değiştirilirse, zincirdeki tüm hash'ler bozulur ve sistem bunu tespit eder.
*   **MongoDB Altyapısı**: Bloklar JSON formatında MongoDB veritabanında saklanır. Yüksek performanslı okuma/yazma sağlar.
*   **Esnek Veri Modeli**: Şu an sohbet logları için ayarlanmıştır ancak `ChatLog` modeli değiştirilerek her türlü veri (Finansal kayıtlar, Tedarik zinciri, Loglar vb.) saklanabilir.

---

## 🛠 Teknik Mimari ve Çalışma Mantığı

### 1. Otorite Anahtarları (`authority_keys.xml`)
Sistem ilk kez çalıştırıldığında `KeyService`, `authority_keys.xml` adında bir dosya oluşturur. Bu dosya, sistemin **dijital kimliğidir (RSA Anahtar Çifti)**.
*   Bu dosya silinirse veya değiştirilirse, sistem daha önce imzaladığı blokları tanıyamaz ve zincir "güvensiz" olarak işaretlenir.
*   Bu anahtar sayesinde hash'i **imzalayarak** verinin kaynağından emin olunur.

### 2. Blok Yapısı ve Zincirleme
Her veri bloğu şunları içerir:
*   **Index**: Sıra numarası.
*   **Timestamp**: Kayıt zamanı.
*   **Data**: Saklanan asıl veri (şu an `ChatLog` nesnesi).
*   **PreviousHash**: Bir önceki bloğun SHA-256 özeti. Bu, zinciri birbirine bağlar.
*   **Hash**: Bloğun kendi özeti.
*   **ValidatorSignature**: Otoritenin bu bloğu onayladığına dair RSA imzası.

### 3. Doğrulama (Audit)
Sistem `/api/ledger/audit` endpoint'i ile tüm zinciri baştan sona tarar:
1.  Veri bütünlüğü bozulmuş mu? (Hash kontrolü)
2.  İmza geçerli mi? (Yetkili kişi mi yazmış?)
3.  Zincir kopuk mu? (PreviousHash kontrolü)

---

## ⚙️ Kurulum ve Ayarlar

### Gereksinimler
*   .NET 8.0 veya üzeri
*   MongoDB (Yerel veya Bulut)

### Konfigürasyon
MongoDB bağlantı adresini `appsettings.json` dosyasından ayarlayabilirsiniz:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017"
  }
}
```

### Veri Modelini Değiştirme
Varsayılan olarak `Models/ChatLog.cs` içinde sohbet verisi (Sender, Message) tutulur. Kendi projenize uyarlamak için:
1.  `Models/ChatLog.cs` dosyasını açın.
2.  İstediğiniz alanları (Örn: `Amount`, `TransactionId`, `SensorData`) ekleyin.
3.  Proje, yeni modelinizi otomatik olarak blok içine gömmeye başlayacaktır.

---

## 🔌 API Kullanımı

Uygulama iki temel API endpoint'i sunar:

### 1. Veri Ekleme (İmzalama)
Yeni bir veriyi blockchain'e yazar.

*   **URL**: `POST /api/ledger/sign`
*   **Body (JSON)**:
    ```json
    {
      "sender": "Ahmet",
      "message": "Merhaba Dünya, bu veri blockchain'e yazılacak."
    }
    ```

### 2. Zinciri Denetleme (Audit)
Tüm veritabanını tarar ve bütünlüğünü doğrular.

*   **URL**: `GET /api/ledger/audit`
*   **Başarılı Yanıt**:
    ```json
    {
      "status": "OK",
      "msg": "Tüm sohbet geçmişi güvenli ve doğrulanabilir."
    }
    ```