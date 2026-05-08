import { mount } from '@vue/test-utils';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import RealTimeMap from '../src/components/RealTimeMap.vue';
import L from 'leaflet';

// Mocking Leaflet
vi.mock('leaflet', () => ({
  default: {
    map: vi.fn().mockReturnValue({
      setView: vi.fn().mockReturnThis(),
      remove: vi.fn(),
      flyTo: vi.fn(),
    }),
    tileLayer: vi.fn().mockReturnValue({
      addTo: vi.fn().mockReturnThis(),
    }),
    marker: vi.fn().mockReturnValue({
      addTo: vi.fn().mockReturnThis(),
      setLatLng: vi.fn(),
    }),
    Icon: {
      Default: {
        prototype: {},
        mergeOptions: vi.fn(),
      }
    }
  }
}));

// Mocking global fetch
global.fetch = vi.fn();

describe('RealTimeMap.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders map container', () => {
    const wrapper = mount(RealTimeMap);
    expect(wrapper.find('div').exists()).toBe(true);
  });

  it('initializes map on mount', () => {
    mount(RealTimeMap);
    expect(L.map).toHaveBeenCalled();
    expect(L.tileLayer).toHaveBeenCalled();
    expect(L.marker).toHaveBeenCalled();
  });

  it('calls Nominatim API when city is updated', async () => {
    const wrapper = mount(RealTimeMap, {
      props: {
        city: 'New York',
        country: 'USA'
      }
    });

    fetch.mockResolvedValueOnce({
      json: () => Promise.resolve([{ lat: '40.7128', lon: '-74.0060' }])
    });

    // Wait for debounce timer (1000ms)
    vi.useFakeTimers();
    
    // Trigger watcher by updating city
    await wrapper.setProps({ city: 'London' });
    
    vi.advanceTimersByTime(1000);
    
    expect(fetch).toHaveBeenCalledWith(expect.stringContaining('city=London'));
    expect(fetch).toHaveBeenCalledWith(expect.stringContaining('country=USA'));
    
    vi.useRealTimers();
  });
});
