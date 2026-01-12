const apiBase = "http://localhost:5231/api/quiz";
const playerName = localStorage.getItem("playerName"); // Dohvati ime iz localStorage
const email = localStorage.getItem("email");
let counter = 0; // Brojač pitanja
let totalScore = 0; // Ukupni poeni igrača
let localResults = []; // Privremeno čuvanje rezultata dok kviz traje
let usedQuestions = new Set(); // Skup za praćenje korišćenih pitanja

if (!playerName || !email) {
  // Ako nema podataka o korisniku, vrati na login
  window.location.href = "index.html";
}

// Dugme za slanje odgovora
document.getElementById("submitAnswer").addEventListener("click", async () => {
  const questionId = localStorage.getItem("currentQuestionId");
  const answerPlayer = document.getElementById("answer").value;

  try {
    const response = await fetch(`${apiBase}/answer`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ QuestionId: questionId, PlayerName: playerName, Answer: answerPlayer }),
    });

    if (!response.ok) {
      throw new Error(`HTTP greška: ${response.status}`);
    }

    if(!answerPlayer)
    {
      alert("Morate uneti odgovor!");
      return;
    }

    const result = await response.json();
    const feedback = result.isCorrect
      ? `Tačno! Osvojili ste ${result.pointsAwarded} poena.`
      : `Netačno. Tačan odgovor je: ${result.correctAnswer}.`;

    // Dodaj rezultat u lokalnu listu
    localResults.push({ questionId, isCorrect: result.isCorrect, pointsAwarded: result.pointsAwarded });
    totalScore += result.pointsAwarded;

    document.getElementById("feedback").textContent = feedback;

    // Sakrij dugme "Submit" i prikaži dugme "Dalje"
    document.getElementById("submitAnswer").classList.add("hidden");
    document.getElementById("nextQuestion").classList.remove("hidden");

  } catch (error) {
    console.error("Greška prilikom slanja odgovora:", error);
    document.getElementById("feedback").textContent =
      "Došlo je do greške prilikom slanja odgovora. Pokušajte ponovo.";
  }
});

// Dugme za prelazak na sledeće pitanje
document.getElementById("nextQuestion").addEventListener("click", () => {
  counter++;

  // Sakrij dugme "Dalje" i ponovo prikaži dugme "Submit"
  document.getElementById("nextQuestion").classList.add("hidden");
  document.getElementById("submitAnswer").classList.remove("hidden");

  loadQuestion();
});

// Funkcija za generisanje nasumičnog ID-a pitanja
function getRandomQuestionId() {
  let randomId;
  do {
    randomId = Math.floor(Math.random() * 21); // Nasumično od 0 do 20
  } while (usedQuestions.has(randomId));
  usedQuestions.add(randomId);
  return randomId;
}

// Funkcija za učitavanje pitanja
async function loadQuestion() {
  if (counter >= 5) {
    // Prikaz finalnog submit dugmeta na kraju kviza
    document.getElementById("submitAnswer").classList.add("hidden");
    document.getElementById("nextQuestion").classList.add("hidden");
    document.getElementById("finalSubmit").classList.remove("hidden");
    return;
  }

  const questionId = getRandomQuestionId();

  try {
    const response = await fetch(`${apiBase}/question/${questionId}`);

    if (!response.ok) {
      throw new Error(`HTTP greška: ${response.status}`);
    }

    const data = await response.json();

    // Postavi pitanje i očisti prethodni odgovor
    document.getElementById("question").textContent = data.question;
    document.getElementById("answer").value = "";

    // Sačuvaj trenutni ID pitanja
    localStorage.setItem("currentQuestionId", data.id);

    // Resetuj feedback
    document.getElementById("feedback").textContent = "";
  } catch (error) {
    console.error("Greška prilikom učitavanja pitanja:", error);
    document.getElementById("question").textContent =
      "Došlo je do greške prilikom učitavanja pitanja. Pokušajte ponovo.";
  }
}

// Dugme za finalni submit rezultata
document.getElementById("finalSubmit").addEventListener("click", async () => {
  try {
    const response = await fetch(`${apiBase}/submit-final-score`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        PlayerName: playerName,
        TotalScore: totalScore,
        Results: localResults, // Pošalji i detalje o pitanjima i rezultatima
      }),
    });

    if (!response.ok) {
      throw new Error(`HTTP greška: ${response.status}`);
    }

    document.getElementById("feedback").textContent =
      "Kviz završen! Vaš rezultat je uspešno sačuvan.";

    window.location.href = "leaderboard.html";

  } catch (error) {
    console.error("Greška prilikom slanja konačnih rezultata:", error);
    document.getElementById("feedback").textContent =
      "Došlo je do greške prilikom slanja konačnih rezultata. Pokušajte ponovo.";
  }
});

// Prvo pitanje se učitava odmah po učitavanju stranice
loadQuestion();