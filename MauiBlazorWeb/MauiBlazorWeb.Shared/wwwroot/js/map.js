const maps = new WeakMap(); // store map instances per element

function initMap(element, initialMarkers) {
    if (!maps.has(element)) {
        const map = L.map(element).setView([0, 0], 2);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);
        maps.set(element, map);
    }
    updateMarkers(element, initialMarkers);
}

function updateMarkers(element, markers) {
    const map = maps.get(element);
    if (!map) return;

    // Remove existing markers
    map.eachLayer(layer => {
        if (layer instanceof L.Marker) map.removeLayer(layer);
    });

    // Add new markers
    markers.forEach(m => {
        L.marker([m.latitude, m.longitude])
            .addTo(map)
            .bindPopup(m.info);
    });

    // Adjust view
    if (markers.length > 0) {
        const bounds = L.latLngBounds(markers.map(m => [m.latitude, m.longitude]));
        map.fitBounds(bounds, { padding: [50, 50] });
    }
}

function getAngle(from, to) {
    const dy = to.latitude - from.latitude;
    const dx = to.longitude - from.longitude;
    const rad = Math.atan2(dy, dx);
    return rad * (180 / Math.PI);
}

function drawArrow(element, from, to, options = {}) {
    const map = maps.get(element);
    if (!map) return;

    // Draw a polyline from 'from' to 'to'
    const arrowLine = L.polyline([
        [from.latitude, from.longitude],
        [to.latitude, to.longitude]
    ], {
        color: options.color || 'red',
        weight: options.weight || 4,
        ...options
    }).addTo(map);

    return arrowLine;
}
