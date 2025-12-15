let map;

function initMap(initialMarkers) {
    if (!map) {
        map = L.map('map').setView([0, 0], 2);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);
    }
    updateMarkers(initialMarkers);
}

function updateMarkers(markers) {
    if (!map) return;

    // Remove existing markers
    map.eachLayer((layer) => {
        if (layer instanceof L.Marker) map.removeLayer(layer);
    });

    // Add new markers
    markers.forEach(m => {
        L.marker([m.latitude, m.longitude])
            .addTo(map)
            .bindPopup(m.info);
    });

    // Optional: Adjust map view to fit markers
    if (markers.length > 0) {
        const bounds = L.latLngBounds(markers.map(m => [m.latitude, m.longitude]));
        map.fitBounds(bounds, { padding: [50, 50] });
    }
}
