// Store map instances and their layers per DOM element
const maps = new WeakMap();
const markerLayers = new WeakMap();
const arrowLayers = new WeakMap();

/**
 * Initializes the map if it does not exist yet
 */
function initMap(element, initialMarkers) {
    if (!maps.has(element)) {
        const map = L.map(element).setView([0, 0], 2);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        maps.set(element, map);
        markerLayers.set(element, L.layerGroup().addTo(map));
        arrowLayers.set(element, L.layerGroup().addTo(map));
    }

    updateMarkers(element, initialMarkers);
}

/**
 * Updates markers and adjusts the viewport
 */
function updateMarkers(element, markers) {
    const map = maps.get(element);
    const markerLayer = markerLayers.get(element);

    if (!map || !markerLayer) return;

    // Clear existing markers
    markerLayer.clearLayers();

    // Add new markers
    markers.forEach(m => {
        L.marker([m.latitude, m.longitude])
            .bindPopup(m.info)
            .addTo(markerLayer);
    });

    // Fit bounds
    if (markers.length > 0) {
        const bounds = L.latLngBounds(
            markers.map(m => [m.latitude, m.longitude])
        );
        map.fitBounds(bounds, { padding: [50, 50] });
    }
}

/**
 * Removes all arrows from the map
 */
function clearArrows(element) {
    const arrowLayer = arrowLayers.get(element);
    if (arrowLayer) {
        arrowLayer.clearLayers();
    }
}

/**
 * Draws a route arrow between two points
 */
function drawArrow(element, from, to, options = {}) {
    const arrowLayer = arrowLayers.get(element);
    if (!arrowLayer) return;

    return L.polyline(
        [
            [from.latitude, from.longitude],
            [to.latitude, to.longitude]
        ],
        {
            color: options.color || 'red',
            weight: options.weight || 4,
            ...options
        }
    ).addTo(arrowLayer);
}
