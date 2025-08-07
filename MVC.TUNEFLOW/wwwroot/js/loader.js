
// LOADER - Configuración de Pace.js
paceOptions = {
    ajax: true,      // Monitorea peticiones AJAX
    document: true,  // Monitorea la carga del DOM
    eventLag: false  // Reduce el retardo de eventos
};

// Cuando Pace.js indica que la página ha terminado de cargar
Pace.on('done', function () {
    // Animación 
    gsap.to('.p', 1, {
        // Animación 
        opacity: 0,   // Desvanecer los elementos '.p'
        y: '-15%',    // Moverlos ligeramente hacia arriba
        stagger: -.1, // Pequeño retraso entre cada elemento '.p' para un efecto escalonado
    });

    // Animación del contenedor principal
    gsap.to('#preloader', 1.5, {
        y: '-100%',             // Deslizar el preloader hacia arriba, fuera de la vista
        ease: 'Expo.easeInOut', // Tipo de animación para un movimiento suave
        delay: 1,               // Retraso de 1 segundo antes de que el preloader comience a desaparecer (para que las animaciones internas terminen)
        onComplete: function () {
            // Función que se ejecuta una vez que la animación de #preloader ha terminado
            // Eliminar el preloader del DOM para liberar recursos y evitar interacciones accidentales
            const preloader = document.getElementById('preloader');
            if (preloader) {
                preloader.remove(); // Elimina el elemento #preloader del HTML
            }

            // Animaciones para elementos con clase 'text' y 'img'
            $('.text').each(function () {
                $(this).delay(1200).addClass('reveal');
            });
            $('.img').each(function () {
                $(this).delay(1200).addClass('reveal');
            });

            // Animaciones condicionales para ciertos IDs o clases, también se ejecutan al cargar
            if (document.querySelector('#index-two') || document.querySelector('#index-one')) {
                gsap.to('.new-release', 0, { opacity: 1 });
                $('.new-release').delay(2000).addClass('opacity');
            }

            if (document.querySelector('.fade-in')) {
                gsap.to('.fade-in', 1, { delay: 1, opacity: 1, stagger: .4 });
            }

            if (document.querySelector('.opacity-contact')) {
                gsap.to('.opacity-contact', 1, { delay: 1, opacity: 1, stagger: .4 });
            }

            // Animación para elementos con clase 'menu-bar-line'
            $('.menu-bar-line').delay(2000).addClass('opacity');

            // Animaciones específicas para la "songs page" que se ejecutan al cargar
            if (document.querySelector('.fade-up')) {
                gsap.to('.fade-up', 1, { opacity: 1, y: 0, delay: 1, stagger: .1 });
            }

            if (document.querySelector('.music-indicator')) {
                gsap.to('.music-indicator', 1, { opacity: 1, delay: 1 });
            }
            if (document.querySelector('.scale')) {
                gsap.to('.scale', 1, { opacity: 1, delay: 1, scale: 1, stagger: .2 });
            }
        }
    });
});