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

/**
 * Gets route information from OSRM API (street-following routing)
 * @param {Object} from - Start point with latitude and longitude
 * @param {Object} to - End point with latitude and longitude
 * @returns {Promise<Object>} Route information including distance, duration, and success status
 */
async function getRouteInfo(from, to) {
    try {
        // OSRM API endpoint (free public server)
        // Format: /route/v1/driving/{lon},{lat};{lon},{lat}
        const url = `https://router.project-osrm.org/route/v1/driving/${from.longitude},${from.latitude};${to.longitude},${to.latitude}?overview=full&geometries=geojson`;

        // Add timeout to prevent hanging
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 10000); // 10 second timeout

        try {
            const response = await fetch(url, { signal: controller.signal });
            clearTimeout(timeoutId);

            if (!response.ok) {
                console.error('OSRM API error:', response.status, response.statusText);
                return {
                    distance: 0,
                    duration: 0,
                    success: false
                };
            }

            const data = await response.json();

        if (data.code === 'Ok' && data.routes && data.routes.length > 0) {
            const route = data.routes[0];

            return {
                distance: route.distance,      // Distance in meters
                duration: route.duration,      // Duration in seconds
                success: true
            };
        } else {
            console.error('OSRM returned no valid route:', data.code);
            return {
                distance: 0,
                duration: 0,
                success: false
            };
        }
        } catch (fetchError) {
            clearTimeout(timeoutId);
            if (fetchError.name === 'AbortError') {
                console.error('OSRM API timeout');
            } else {
                throw fetchError;
            }
            return {
                distance: 0,
                duration: 0,
                success: false
            };
        }
    } catch (error) {
        console.error('Error calling OSRM API:', error);
        return {
            distance: 0,
            duration: 0,
            success: false
        };
    }
}

// Global request queue for OSRM API calls
let osrmRequestQueue = Promise.resolve();
let osrmLastRequestTime = 0;
const OSRM_MIN_DELAY_MS = 500; // Minimum delay between requests (OSRM has ~2 req/sec limit)

/**
 * Delay helper function
 */
function delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Draws a route on the map using OSRM routing (follows streets)
 * Includes throttling to respect OSRM API rate limits (~2 requests/second)
 * @param {HTMLElement} element - The map element
 * @param {Object} from - Start point with latitude and longitude
 * @param {Object} to - End point with latitude and longitude
 * @param {Object} options - Optional styling options
 * @returns {Promise<Object>} Route information
 */
async function drawRouteWithOSRM(element, from, to, options = {}) {
    const map = maps.get(element);
    const arrowLayer = arrowLayers.get(element);

    if (!map || !arrowLayer) {
        return { success: false, distance: 0, duration: 0 };
    }

    // Throttle requests using a queue to respect API rate limits
    return new Promise((resolve) => {
        osrmRequestQueue = osrmRequestQueue.then(async () => {
            // Ensure minimum delay between requests
            const timeSinceLastRequest = Date.now() - osrmLastRequestTime;
            if (timeSinceLastRequest < OSRM_MIN_DELAY_MS) {
                await delay(OSRM_MIN_DELAY_MS - timeSinceLastRequest);
            }
            osrmLastRequestTime = Date.now();

            // Try with exponential backoff
            const maxRetries = 3;
            for (let attempt = 0; attempt < maxRetries; attempt++) {
                try {
                    // Get route from OSRM with timeout
                    const url = `https://router.project-osrm.org/route/v1/driving/${from.longitude},${from.latitude};${to.longitude},${to.latitude}?overview=full&geometries=geojson`;

                    // Create AbortController for timeout
                    const controller = new AbortController();
                    const timeoutId = setTimeout(() => controller.abort(), 10000); // 10 second timeout

                    try {
                        const response = await fetch(url, { signal: controller.signal });
                        clearTimeout(timeoutId);
                        const data = await response.json();

                    if (data.code === 'Ok' && data.routes && data.routes.length > 0) {
                        const route = data.routes[0];

                        // Convert GeoJSON coordinates to Leaflet format [lat, lng]
                        const coordinates = route.geometry.coordinates.map(coord => [coord[1], coord[0]]);

                        // Draw the route on the map
                        L.polyline(coordinates, {
                            color: options.color || 'blue',
                            weight: options.weight || 4,
                            opacity: options.opacity || 0.7,
                            ...options
                        }).addTo(arrowLayer);

                        const result = {
                            distance: route.distance,
                            duration: route.duration,
                            success: true
                        };
                        resolve(result);
                        return result;
                    } else {
                        console.error('OSRM returned no valid route');
                        const result = { success: false, distance: 0, duration: 0 };
                        resolve(result);
                        return result;
                    }
                    } catch (fetchError) {
                        clearTimeout(timeoutId);
                        throw fetchError;
                    }
                } catch (error) {
                    const errorMsg = error.name === 'AbortError' ? 'Request timeout' : error.message;
                    console.error(`OSRM API attempt ${attempt + 1} failed:`, errorMsg);

                    // Retry with exponential backoff
                    if (attempt < maxRetries - 1) {
                        const backoffDelay = Math.pow(2, attempt) * 500; // 500ms, 1s, 2s
                        console.log(`Retrying in ${backoffDelay}ms...`);
                        await delay(backoffDelay);
                    } else {
                        // All retries failed
                        const result = { success: false, distance: 0, duration: 0 };
                        resolve(result);
                        return result;
                    }
                }
            }
        });
    });
}
