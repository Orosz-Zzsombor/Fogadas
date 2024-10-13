
Fogadási rendszer - Asztali alkalmazás

Ez egy fogadási rendszer asztali alkalmazás, amely lehetőséget biztosít a felhasználóknak sporteseményekre való fogadások megtételére. Az alkalmazás támogatja a különböző szerepköröket, mint például a szervező, adminisztrátor és felhasználó, és lehetőséget nyújt a sportesemények kezelésére, fogadások lebonyolítására, valamint egy felhasználói egyenleg követésére.

Funkciók

- Felhasználói regisztráció és bejelentkezés: A felhasználók fiókot hozhatnak létre, bejelentkezhetnek, és fogadásokat tehetnek.
- Sportesemények megjelenítése és kezelése: Az események kategóriák szerint listázhatók, és lehetőség van ezek rendezésére dátum és kategória alapján.
- Fogadás készítése: A felhasználók fogadásokat tehetnek elérhető eseményekre.
- Adminisztráció: A szervezők új eseményeket hozhatnak létre, módosíthatják és lezárhatják azokat.
- Egyenleg követés: A felhasználói egyenleg frissül minden fogadás után, a nyeremények jóváírása megtörténik.

Telepítés

1. Klónozd le a repót a gépedre:

2. Nyisd meg a projektet egy C#-fejlesztői környezetben, például a Visual Studio-ban.

3. Telepítsd a szükséges csomagokat és függőségeket:
   - MySQL.Data a MySQL adatbázis kezeléséhez
   - System.Windows.Controls az asztali felülethez

4. Futtasd a projektet a fejlesztői környezetből.

Adatbázis beállítása

A projekt MySQL adatbázist használ. A szükséges táblák létrehozása automatikusan megtörténik az alkalmazás elindításakor. A kapcsolati adatokat az `App.config` fájlban adhatod meg.

Példa adatbázis kapcsolat:

```csharp
private string connectionString = "Server=localhost;Database=FogadasDB;Uid=root;Pwd=;";
```

Táblák létrehozása

Az alábbi SQL parancsok futtatásával létrejönnek a szükséges táblák az adatbázisban:

```sql
CREATE TABLE IF NOT EXISTS Bettors (
    BettorsID INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL,
    Password VARCHAR(255),
    Balance INT NOT NULL,
    Email VARCHAR(100) NOT NULL,
    JoinDate DATE NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    Role ENUM('user', 'admin', 'organizer') NOT NULL DEFAULT 'user'
);

CREATE TABLE IF NOT EXISTS Events (
    EventID INT AUTO_INCREMENT PRIMARY KEY,
    EventName VARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Category VARCHAR(50) NOT NULL,
    Location VARCHAR(100) NOT NULL,
    IsClosed BOOLEAN NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Bets (
    BetsID INT AUTO_INCREMENT PRIMARY KEY,
    BetDate DATE NOT NULL,
    Odds FLOAT NOT NULL,
    Amount INT NOT NULL,
    BettorsID INT NOT NULL,
    EventID INT NOT NULL,
    Status BOOLEAN NOT NULL,
    FOREIGN KEY (BettorsID) REFERENCES Bettors(BettorsID),
    FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
```

Példa adatok beillesztésére

Az alábbi SQL kóddal például hozzáadhatsz alapértelmezett felhasználókat (adminisztrátor, szervező):


```sql
INSERT INTO Events (EventName, EventDate, Category, Location,IsClosed) VALUES
('Champions League Final', '2025-05-25', 'Football', 'Wembley Stadium, London',1),
('NBA Finals Game 7', '2025-06-15', 'Basketball', 'Staples Center, Los Angeles',1),
('Wimbledon Finals', '2025-07-14', 'Tennis', 'All England Club, London',1),
('Formula 1 Grand Prix', '2025-10-07', 'Motorsport', 'Monza Circuit, Italy',0);
```
Szerepkörök és jogosultságok tervezete:

User:

Regisztrálhat, fogadásokat tehet, egyenlegét kezelheti.
Megtekintheti saját fogadásait és azok státuszát.
Nem fér hozzá adminisztrációs funkciókhoz (pl. új események létrehozása, felhasználók kezelése).

Organizer:

Minden jogot megkap, amit a User szerepkör is, de ezen felül:
Felülvizsgálhatja és módosíthatja a fogadások státuszát.
Új eseményeket hozhat létre vagy törölhet.
Módosíthatja az események adatait (pl. időpont, helyszín).

Admin:

Láthatja más felhasználók fogadásait.
Teljes hozzáférése van az összes felhasználó adatához, beleértve azok egyenlegét, státuszát, és jelszavait (hash-elve).
Új felhasználó, meglévő törlése, jelszó alaphelyzetbe hozása, 
Adminisztrálhatja a felhasználókat (pl. letilthatja őket, vagy aktiválhatja a fiókjukat).

```sql
INSERT INTO Bettors (Username, Password, Email, Balance, JoinDate, IsActive, Role) 
VALUES 
('organizer', '0bf7ab78559a04941f158a11b00afaf6a8b22f90cff387edbc8e1d7a9b99cca0', 'organizer@gmail.com', 0, '2024-10-13', 1, 'organizer'),
('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'admin@gmail.com', 0, '2024-10-13', 1, 'admin'),
('alany', 'e4af01de22a4fc2cf405fb9fec12dfe84498a4dd67423364485f4c74dcf00bd3', 'alany@gmail.com', 0, '2024-10-13', 1, 'user');

--A Jelszó megegyezik a felhasználó nevével.
```

Használat

1. Regisztráció: A felhasználók regisztrálhatnak és létrehozhatják saját profiljukat.
2. Bejelentkezés: Az alkalmazás lehetőséget nyújt a felhasználók számára a bejelentkezésre.
3. Események megtekintése: A felhasználók böngészhetik az elérhető sporteseményeket.
4. Fogadások: A bejelentkezett felhasználók fogadásokat tehetnek a kiválasztott eseményekre.
5. Szervezők: Új eseményeket hozhatnak létre, módosíthatják és lezárhatják a meglévőket.

Fejlesztési környezet

- C# és WPF az asztali alkalmazás megvalósításához
- MySQL az adatbázis kezeléséhez



