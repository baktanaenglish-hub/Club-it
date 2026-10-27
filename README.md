# Club it

Club it מרכזת במקום אחד את ההטבות שמגיעות למשק הבית ממועדונים, מכרטיסי אשראי ומגורמים נוספים (מקס, כאל, פיס וכו').
מקלידים שם של חנות, ומקבלים מיד את ההטבות שבתוקף מהמועדונים שסימנתם בהגדרות, עם כפתור שמוביל למימוש ההטבה.

הקוד מממש את שלב 1 (MVP אישי) לפי מסמך האפיון, ובנוי כבר עכשיו לריבוי משתמשים ומשקי בית לקראת שלב 2.

## מבנה הפתרון

| פרויקט | תפקיד |
|---|---|
| `src/ClubIt.Domain` | ישויות (Club, Category, Merchant, MerchantAlias, Branch, Benefit, BenefitBranch, Household, HouseholdClub) ונרמול שמות לחיפוש |
| `src/ClubIt.Contracts` | DTOs משותפים לשרת ולאפליקציה |
| `src/ClubIt.Application` | לוגיקה עסקית: חיפוש, הטבות, רשתות וסניפים, הגדרות משק בית |
| `src/ClubIt.Infrastructure` | EF Core (PostgreSQL + מיגרציות), ASP.NET Core Identity, JWT, נתוני בסיס |
| `src/ClubIt.Api` | ASP.NET Core Web API |
| `src/ClubIt.Mobile.Core` | לקוח API ו-ViewModels לכל המסכים, ללא תלות ב-MAUI (ניתן לבדיקה) |
| `src/ClubIt.Mobile` | אפליקציית ‎.NET MAUI לאנדרואיד ו-iOS, בעברית וב-RTL |
| `tests/ClubIt.Tests` | בדיקות אינטגרציה לשרת ול-ViewModels מול שרת אמיתי בזיכרון |

האפליקציה מדברת רק עם ה-API, ולא ישירות עם מסד הנתונים.

## דרישות

- .NET SDK 10
- PostgreSQL 16 (או Docker)
- לאפליקציה: ה-workload של MAUI (`dotnet workload install maui`). כדי לבנות ל-iOS צריך Mac.

## הרצת השרת

```bash
docker compose up -d                 # PostgreSQL מקומי (clubit/clubit)
dotnet tool restore
dotnet run --project src/ClubIt.Api  # http://localhost:5136
```

בעלייה השרת מריץ את המיגרציות ומזין את נתוני הבסיס: מועדונים, קטגוריות, רשתות נפוצות וכמה סניפים.
את ההטבות עצמן משק הבית מזין ידנית (שלב 1).

כדי לנסות בלי PostgreSQL אפשר להריץ עם SQLite:

```bash
cd src/ClubIt.Api
Database__Provider=Sqlite ConnectionStrings__ClubIt="Data Source=clubit.db" dotnet run
```

### הגדרות

| מפתח | ברירת מחדל | הערות |
|---|---|---|
| `ConnectionStrings:ClubIt` | PostgreSQL מקומי | |
| `Database:Provider` | `Postgres` | `Sqlite` לניסוי מקומי (בלי מיגרציות) |
| `Jwt:SigningKey` | ריק | **חובה**, לפחות 32 תווים. בפיתוח מוגדר ב-`appsettings.Development.json`. בייצור יש לספק אותו כסוד (למשל משתנה סביבה `Jwt__SigningKey`) |
| `Jwt:ExpirationDays` | 30 | |

השרת לא יעלה אם מפתח ה-JWT חסר או קצר מדי.

## הרצת האפליקציה

```bash
dotnet build src/ClubIt.Mobile -f net10.0-android -t:Run   # אמולטור אנדרואיד
dotnet build src/ClubIt.Mobile -f net10.0-ios -t:Run       # סימולטור iOS (ב-Mac)
```

בגרסת Debug האפליקציה פונה לשרת שרץ על מחשב הפיתוח (`10.0.2.2:5136` באמולטור אנדרואיד, `localhost:5136` ב-iOS).
את הכתובת לגרסת Release מגדירים ב-`MauiProgram.ApiBaseAddress`.

## בדיקות

```bash
dotnet test ClubIt.Backend.slnf
```

`ClubIt.Backend.slnf` כולל את כל הפרויקטים חוץ מאפליקציית MAUI, כך שאפשר לבנות ולבדוק בלי ה-workload של MAUI.
הבדיקות מריצות את ה-API מול SQLite זמני, ומכסות הרשמה והצטרפות למשק בית, הרשאות בין משקי בית, חיפוש לפי שם, כינוי וסניף, תוקף, מיון, לשונית יציאות, וה-ViewModels של האפליקציה.

מיגרציה חדשה אחרי שינוי במודל:

```bash
dotnet ef migrations add <Name> --project src/ClubIt.Infrastructure --startup-project src/ClubIt.Infrastructure --output-dir Persistence/Migrations
```

## API

כל ה-endpoints חוץ מהרשמה והתחברות דורשים `Authorization: Bearer <JWT>`. שגיאות מוחזרות כ-Problem Details עם הודעות בעברית.

| Method | Path | תיאור |
|---|---|---|
| POST | `/api/auth/register` | הרשמה: יצירת משק בית (`householdName`) או הצטרפות לקיים (`inviteCode`) |
| POST | `/api/auth/login` | התחברות, מחזיר JWT |
| GET | `/api/clubs` | כל המועדונים |
| GET | `/api/household` | משק הבית, קוד ההצטרפות והחברים בו |
| GET / PUT | `/api/household/clubs` | המועדונים של משק הבית |
| GET | `/api/benefits/search?q={name}&branchId={id}` | חיפוש הטבות לפי חנות, סניף אופציונלי |
| GET | `/api/benefits/entertainment?clubId={id}` | הטבות יציאות, סינון אופציונלי לפי מועדון |
| GET / PUT / DELETE | `/api/benefits/{id}` | פרטי הטבה, עדכון, מחיקה |
| POST | `/api/benefits` | הוספת הטבה |
| GET | `/api/merchants?q={name}` | חיפוש רשתות (השלמה אוטומטית) |
| POST | `/api/merchants` | הוספת רשת עם כינויים |
| PUT | `/api/merchants/{id}` | עדכון רשת וכינויים |
| GET / POST | `/api/merchants/{id}/branches` | סניפים של רשת, הוספת סניף |
| GET | `/api/categories` | קטגוריות |

בסביבת Development מסמך OpenAPI זמין ב-`/openapi/v1.json`.

## החלטות מימוש

- **חיפוש:** לכל רשת וכינוי נשמר שם מנורמל (אותיות קטנות, בלי רווחים וסימני פיסוק), ולכן "שופר סל", "Shufersal" ו"שופרסל" מוצאים את אותה רשת. ההתאמה חלקית (contains) ורצה במסד הנתונים.
- **כינויים** נשמרים בטבלה נפרדת `MerchantAliases`.
- **בחירת סניף:** כשנבחר סניף מוצגות רק הטבות של הרשת שלו, שתקפות בכל הסניפים או שיש להן רשומה ב-`BenefitBranch` לאותו סניף.
- **מיון:** לפי שם המועדון, ובתוכו לפי תאריך התפוגה הקרוב.
- **תוקף:** `ValidUntil` חובה, והטבה בלעדיו נדחית בשמירה. `ValidFrom` אופציונלי. "היום" מחושב לפי שעון ישראל.
- **הרשאות:** לכל הטבה יש `HouseholdId` של משק הבית שהזין אותה. משתמש רואה ועורך רק את ההטבות של משק הבית שלו. הטבה בלי `HouseholdId` היא הטבה משותפת לכולם, הכנה להטבות הקהילתיות של שלב 2; אפשר לראות אותה אבל לא לערוך. רשתות, סניפים, מועדונים וקטגוריות הם קטלוג משותף.
- **הצטרפות בן/בת זוג:** כל משק בית מקבל קוד הצטרפות בן 8 תווים, שמוצג במסך ההגדרות.
- **אבטחה:** סיסמאות נשמרות דרך Identity, יש נעילה זמנית אחרי 5 ניסיונות כושלים והגבלת קצב על endpoints ההתחברות. באפליקציה ה-JWT נשמר ב-SecureStorage. אין שמירת סיסמאות של מועדונים או כרטיסי אשראי.
- **מימוש הטבה:** רק דרך `RedeemUrl`. קישורי https נפתחים דרך המערכת, כך שקישור אוניברסלי פותח את אפליקציית המועדון אם היא מותקנת. גם קישורי deep link עם scheme משלהם נתמכים.
- `Branch` כולל קווי אורך ורוחב לקראת חיפוש לפי מיקום בשלב 1.5.
