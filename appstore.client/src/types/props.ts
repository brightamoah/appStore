export interface AuthLayoutProps {
  /**
   * Path to the light mode image asset.
   * @default "/images/login.jpg"
   */
  imageLight?: string;
  /**
   * Path to the dark mode image asset.
   * @default "/images/login-dark.jpg"
   */
  imageDark?: string;
  /**
   * Alt text for the visual illustration image.
   * @default "Authentication visual"
   */
  imageAlt?: string;
  /**
   * Target route for the back button.
   * @default "{ name: '/tasks/' }"
   */
  backTo?: string | Record<string, unknown>;
  /**
   * Accessible aria-label for the back button.
   * @default "Go back"
   */
  backLabel?: string;
  /**
   * Whether to display the image on the right and form on the left.
   * @default false
   */
  reverse?: boolean;
  /**
   * Optional quote / testimonial displayed over the hero image.
   */
  quote?: {
    text: string;
    author?: string;
    role?: string;
  };
  /**
   * Whether to show the overlay on the banner.
   * @default true
   */
  showOverlay?: boolean;
}
