
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
INSERT INTO Events (EventName, EventDate, Category, Location, IsClosed) VALUES
('A Manchester United nyerni fog a Chelsea ellen?', '2023-05-25', 'Football', 'Old Trafford, Manchester', 1),
('A Miami Heat megveri a Golden State Warriors-t?', '2025-06-15', 'Basketball', 'FTX Arena, Miami', 1),
('A Serena Williams megnyeri a US Open-t?', '2025-08-29', 'Tennis', 'Arthur Ashe Stadium, New York', 0),
('A Mercedes nyer a Forma-1-es Nagydíjon?', '2025-09-20', 'Motorsport', 'Monaco Grand Prix', 1),
('A Bayern München nyerni fog a Borussia Dortmund ellen?', '2025-10-15', 'Football', 'Allianz Arena, München', 1),
('A Toronto Raptors megveri a Philadelphia 76ers-t?', '2025-11-10', 'Basketball', 'Scotiabank Arena, Toronto', 1),
('A Rafael Nadal megnyeri a Roland Garrost?', '2025-06-04', 'Tennis', 'Stade Roland Garros, Párizs', 1),
('A Red Bull nyer a Forma-1-es Ausztrál Nagydíjon?', '2025-03-30', 'Motorsport', 'Melbourne Grand Prix Circuit', 0),
('A Juventus nyerni fog az Inter Milan ellen?', '2025-04-02', 'Football', 'Allianz Stadium, Torino', 1),
('A LA Dodgers megnyeri a World Series-t?', '2025-10-31', 'Baseball', 'Dodger Stadium, Los Angeles', 1);

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
('alany', 'e4af01de22a4fc2cf405fb9fec12dfe84498a4dd67423364485f4c74dcf00bd3', 'alany@gmail.com', 10000, '2024-10-13', 1, 'user');
('alany1', 'e4af01de22a4fc2cf405fb9fec12dfe84498a4dd67423364485f4c74dcf00bd3', 'alany1@gmail.com', 0, '2024-12-13', 0, 'user');
('alany2', 'e4af01de22a4fc2cf405fb9fec12dfe84498a4dd67423364485f4c74dcf00bd3', 'alany2@gmail.com', 0, '2023-10-13', 1, 'user');
('alany3', 'e4af01de22a4fc2cf405fb9fec12dfe84498a4dd67423364485f4c74dcf00bd3', 'alany3@gmail.com', 0, '2022-01-13', 1, 'user');
('alany4', 'e4af01de22a4fc2cf405fb9fec12dfe84498a4dd67423364485f4c74dcf00bd3', 'alany4@gmail.com', 0, '2022-07-13', 1, 'user');
('alany5', 'e4af01de22a4fc2cf405fb9fec12dfe84498a4dd67423364485f4c74dcf00bd3', 'alany5@gmail.com', 0, '2023-09-13', 1, 'user');

-- A tesztadatok jelszava megegyezik a felhasználónevükkel,
a usereknek "alany" a jelszava.
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



