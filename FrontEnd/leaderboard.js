const apiBase = "http://localhost:5231/api/leaderboard";

// Dohvati trenutni datum u odgovarajućem formatu
const currentDate = new Date().toISOString().split("T")[0]; // Format: yyyy-MM-dd
const currentMonth = currentDate.slice(0, 7); // Format: yyyy-MM

async function loadLeaderboard(type, date, tableId) {
  try {
    const response = await fetch(`${apiBase}/${type}/${date}`);

    if (!response.ok) {
      throw new Error(`HTTP greška: ${response.status}`);
    }

    const data = await response.json();
    const tableBody = document.getElementById(tableId);

    // Očisti tabelu
    tableBody.innerHTML = "";

    // Popuni tabelu
    data.forEach((entry) => {
      const row = `
        <tr>
          <td>${entry.playerName ?? "N/A"}</td>
          <td>${entry.emailAddress ?? "N/A"}</td>
          <td>${entry.score ?? 0}</td>
        </tr>`;
      tableBody.innerHTML += row;
    });
  } catch (error) {
    console.error(`Greška prilikom učitavanja leaderboard-a (${type}):`, error);
  }
}

// Učitaj dnevni, mesečni i all-time leaderboard
loadLeaderboard("daily", currentDate, "dailyLeaderboardTable");
loadLeaderboard("monthly", currentMonth, "monthlyLeaderboardTable");
loadLeaderboard("all-time", currentDate, "allTimeLeaderboardTable");

// Dugme za prelazak na kviz
document.getElementById("playGameButton").addEventListener("click", () => {
  window.location.href = "quiz.html";
});

// Dugme za izlogovanje
document.getElementById("logoutButton").addEventListener("click", () => {
  window.location.href = "index.html"; // Prebaci korisnika na početnu stranicu
});

// Dugme za brisanje rezultata
document.getElementById("clearScoresButton").addEventListener("click", async () => {
  const loggedInPlayer = JSON.parse(localStorage.getItem("loggedInPlayer"));

  if (!loggedInPlayer || !loggedInPlayer.playerName) {
    alert("Nijedan korisnik trenutno nije prijavljen.");
    return;
  }

  const confirmation = confirm(`Da li ste sigurni da želite da obrišete sve rezultate za "${loggedInPlayer.playerName}" sa svih leaderboard-a?`);
  if (!confirmation) return;

  try {
    const response = await fetch(`${apiBase}/clear-scores/${encodeURIComponent(loggedInPlayer.playerName)}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP greška: ${response.status}`);
    }

    // Osveži leaderboard-e
    loadLeaderboard("daily", currentDate, "dailyLeaderboardTable");
    loadLeaderboard("monthly", currentMonth, "monthlyLeaderboardTable");
    loadLeaderboard("all-time", currentDate, "allTimeLeaderboardTable");
    
  } catch (error) {
    console.error("Greška prilikom brisanja rezultata:", error);
    alert(`Došlo je do greške prilikom brisanja rezultata: ${error.message}`);
  }
});

