<template>
  <div v-if="isVisible" class="birthday-overlay">
    <div class="birthday-card">
      <h2>Doğum günün kutlu olsunn Bullboss! 🎉</h2>
      <div class="cake">🎂</div>
      <p>Nice mutlu yıllara!</p>
      <button @click="close" class="close-btn">Teşekkürler!</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const isVisible = ref(false)

onMounted(() => {
  const hasSeen = localStorage.getItem('bullboss_birthday_seen')
  if (!hasSeen) {
    isVisible.value = true
    triggerConfetti()
  }
})

function close() {
  isVisible.value = false
  localStorage.setItem('bullboss_birthday_seen', 'true')
}

function triggerConfetti() {
  const script = document.createElement('script')
  script.src = 'https://cdn.jsdelivr.net/npm/canvas-confetti@1.6.0/dist/confetti.browser.min.js'
  script.onload = () => {
    const duration = 7 * 1000;
    const animationEnd = Date.now() + duration;
    const defaults = { startVelocity: 30, spread: 360, ticks: 60, zIndex: 10000 };

    function randomInRange(min, max) {
      return Math.random() * (max - min) + min;
    }

    const interval = setInterval(function() {
      const timeLeft = animationEnd - Date.now();

      if (timeLeft <= 0) {
        return clearInterval(interval);
      }

      const particleCount = 50 * (timeLeft / duration);
      window.confetti(Object.assign({}, defaults, { particleCount, origin: { x: randomInRange(0.1, 0.3), y: Math.random() - 0.2 } }));
      window.confetti(Object.assign({}, defaults, { particleCount, origin: { x: randomInRange(0.7, 0.9), y: Math.random() - 0.2 } }));
    }, 250);
  }
  document.head.appendChild(script)
}
</script>

<style scoped>
.birthday-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
  backdrop-filter: blur(8px);
  animation: fadeIn 0.5s ease-out;
}

.birthday-card {
  background: white;
  padding: 3rem;
  border-radius: 24px;
  text-align: center;
  box-shadow: 0 20px 50px rgba(0,0,0,0.3);
  animation: popIn 0.7s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  max-width: 90%;
  width: 450px;
  position: relative;
  overflow: hidden;
}

.birthday-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 8px;
  background: linear-gradient(90deg, #ff007f, #ff8c00, #ffd700, #00ff00, #00bfff, #8a2be2);
}

.birthday-card h2 {
  color: #1e293b;
  font-size: 2.2rem;
  margin-bottom: 1rem;
  font-weight: 800;
  line-height: 1.2;
}

.cake {
  font-size: 6rem;
  margin: 1.5rem 0;
  animation: bounce 2s infinite;
}

.birthday-card p {
  color: #64748b;
  font-size: 1.2rem;
  margin-bottom: 2rem;
  font-weight: 600;
}

.close-btn {
  background: linear-gradient(135deg, #4f46e5, #ec4899);
  color: white;
  border: none;
  padding: 1rem 2.5rem;
  border-radius: 12px;
  font-size: 1.1rem;
  font-weight: 700;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  box-shadow: 0 4px 15px rgba(79, 70, 229, 0.4);
}

.close-btn:hover {
  transform: scale(1.05);
  box-shadow: 0 6px 20px rgba(79, 70, 229, 0.6);
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes popIn {
  0% { transform: scale(0.8); opacity: 0; }
  100% { transform: scale(1); opacity: 1; }
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-15px); }
}
</style>
