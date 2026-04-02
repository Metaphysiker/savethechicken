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
 * Gets route information (distance, duration) between two points
 * Returns: { distance: meters, duration: seconds, success: boolean }
 */
async function getRouteInfo(from, to) {
    try {
        const url = `https://router.project-osrm.org/route/v1/driving/${from.longitude},${from.latitude};${to.longitude},${to.latitude}?overview=false`;

        const response = await fetch(url);
        const data = await response.json();

        if (data.code === 'Ok' && data.routes && data.routes.length > 0) {
            return {
                distance: data.routes[0].distance,
                duration: data.routes[0].duration,
                success: true
            };
        } else {
            return { distance: 0, duration: 0, success: false };
        }
    } catch (error) {
        console.error('Error fetching route info:', error);
        return { distance: 0, duration: 0, success: false };
    }
}

/**
 * Draws a route arrow between two points using OSRM routing
 */
async function drawArrow(element, from, to, options = {}) {
    const arrowLayer = arrowLayers.get(element);
    if (!arrowLayer) return;

    try {
        // Use OSRM public demo server (free, no API key needed)
        // For production, consider self-hosting OSRM
        const url = `https://router.project-osrm.org/route/v1/driving/${from.longitude},${from.latitude};${to.longitude},${to.latitude}?overview=full&geometries=geojson`;

        const response = await fetch(url);
        const data = await response.json();

        if (data.code === 'Ok' && data.routes && data.routes.length > 0) {
            // Use actual route geometry from OSRM
            const coordinates = data.routes[0].geometry.coordinates.map(coord => [coord[1], coord[0]]);

            return L.polyline(coordinates, {
                color: options.color || 'blue',
                weight: options.weight || 4,
                opacity: 0.7,
                ...options
            }).addTo(arrowLayer);
        } else {
            // Fallback to straight line if routing fails
            console.warn('OSRM routing failed, using straight line');
            return drawStraightArrow(element, from, to, options);
        }
    } catch (error) {
        // Fallback to straight line on error
        console.error('Error fetching route:', error);
        return drawStraightArrow(element, from, to, options);
    }
}

/**
 * Draws a straight line arrow (fallback)
 */
function drawStraightArrow(element, from, to, options = {}) {
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
