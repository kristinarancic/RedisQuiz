const apiBase = "http://localhost:5231/api/auth";

document.getElementById("loginForm").addEventListener("submit", async (event) => {
  event.preventDefault(); // Spreči reload stranice

  const playerName = document.getElementById("playerName").value;
  const email = document.getElementById("email").value;

  try {
    const response = await fetch(`${apiBase}/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ playerName, email }),
    });

    if (!response.ok) {
      throw new Error(`HTTP greška: ${response.status}`);
    }

    const data = await response.json();

    // Postavi nove podatke u localStorage
    localStorage.setItem("loggedInPlayer", JSON.stringify(data));
    localStorage.setItem("playerName", data.playerName);
    localStorage.setItem("email", data.email);

    // Preusmeri na leaderboard stranicu
    window.location.href = "leaderboard.html";

  } catch (error) {
    console.error("Greška prilikom logovanja:", error);
    alert("Prijava nije uspela. Molimo pokušajte ponovo.");
  }
});