import {
  Component,
  signal,
  computed,
  viewChild,
  ElementRef,
  AfterViewInit,
  effect,
  input,
  output,
  ChangeDetectionStrategy,
} from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { IconComponent } from '../icon/icon.component';
import { SelectorOption } from '../../../core/models/selector.model';
import { Guid } from '../../../core/models/guid.model';

@Component({
  selector: 'app-autocomplete-selector',
  standalone: true,
  imports: [TranslateModule, IconComponent],
  templateUrl: './autocomplete-selector.component.html',
  styleUrl: './autocomplete-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:click)': 'onDocumentClick($event)',
  },
})
export class AutocompleteSelectorComponent implements AfterViewInit {
  options = input<SelectorOption[] | ((searchQuery?: string) => Promise<SelectorOption[]>)>([]);
  placeholder = input('grid.searchOrSelect');
  initialValue = input<Guid | null>(null);
  disabled = input(false);

  valueChange = output<Guid | null>();

  searchInput = viewChild.required<ElementRef<HTMLInputElement>>('searchInput');

  protected readonly searchText = signal('');
  protected readonly availableOptions = signal<SelectorOption[]>([]);
  protected readonly loading = signal(false);
  protected readonly showDropdown = signal(false);
  protected readonly selectedOption = signal<SelectorOption | null>(null);
  protected readonly highlightedIndex = signal(-1);

  private debounceTimer?: ReturnType<typeof setTimeout>;
  private readonly debounceMs = 300;

  // Computed filtered options based on search text
  protected readonly filteredOptions = computed(() => {
    const search = this.searchText().toLowerCase();
    const options = this.availableOptions();
    
    if (!search) return options;
    
    return options.filter(opt => 
      opt.name.toLowerCase().includes(search)
    );
  });

  constructor() {
    // Watch for changes in initialValue
    effect(async () => {
      const initValue = this.initialValue();
      if (initValue) {
        await this.setDisplayValueFromId(initValue);
      } else {
        this.searchText.set('');
        this.selectedOption.set(null);
      }
    });
  }

  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.autocomplete-selector')) {
      this.showDropdown.set(false);
    }
  }

  async ngAfterViewInit() {
    // Load initial options
    await this.loadOptions();
  }

  protected onInput(value: string) {
    this.searchText.set(value);
    this.showDropdown.set(true);
    this.highlightedIndex.set(-1);

    // Clear selection if text changes
    if (this.selectedOption() && this.selectedOption()!.name !== value) {
      this.selectedOption.set(null);
      this.valueChange.emit(null);
    }

    // Clear previous timer
    if (this.debounceTimer) {
      clearTimeout(this.debounceTimer);
    }

    // Set new timer to search after debounce delay
    this.debounceTimer = setTimeout(async () => {
      await this.loadOptions(value || undefined);
    }, this.debounceMs);
  }

  protected onFocus() {
    this.showDropdown.set(true);
  }

  protected selectOption(option: SelectorOption) {
    this.selectedOption.set(option);
    this.searchText.set(option.name);
    this.showDropdown.set(false);
    this.valueChange.emit(option.id);
  }

  protected onKeyDown(event: KeyboardEvent) {
    const options = this.filteredOptions();
    const currentIndex = this.highlightedIndex();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (!this.showDropdown()) {
          this.showDropdown.set(true);
        } else {
          const newIndex = currentIndex < options.length - 1 ? currentIndex + 1 : 0;
          this.highlightedIndex.set(newIndex);
          this.scrollToHighlighted();
        }
        break;

      case 'ArrowUp':
        event.preventDefault();
        if (this.showDropdown()) {
          const newIndex = currentIndex > 0 ? currentIndex - 1 : options.length - 1;
          this.highlightedIndex.set(newIndex);
          this.scrollToHighlighted();
        }
        break;

      case 'Enter':
        event.preventDefault();
        if (this.showDropdown() && currentIndex >= 0 && options[currentIndex]) {
          this.selectOption(options[currentIndex]);
        }
        break;

      case 'Escape':
        event.preventDefault();
        this.showDropdown.set(false);
        this.highlightedIndex.set(-1);
        break;
    }
  }

  protected clearSelection() {
    this.searchText.set('');
    this.selectedOption.set(null);
    this.showDropdown.set(false);
    this.valueChange.emit(null);
    this.searchInput().nativeElement.focus();
  }

  private scrollToHighlighted() {
    setTimeout(() => {
      const highlighted = document.querySelector('.autocomplete-option.highlighted');
      if (highlighted) {
        highlighted.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
      }
    }, 0);
  }

  private async loadOptions(searchQuery?: string) {
    this.loading.set(true);
    try {
      const opts = this.options();
      if (typeof opts === 'function') {
        const result = await opts(searchQuery);
        this.availableOptions.set(result);
      } else {
        // Static options, no server-side filtering needed
        this.availableOptions.set(opts);
      }
    } catch (error) {
      console.error('Error loading autocomplete options:', error);
      this.availableOptions.set([]);
    } finally {
      this.loading.set(false);
    }
  }

  private async setDisplayValueFromId(id: Guid) {
    // First ensure options are loaded
    if (this.availableOptions().length === 0) {
      await this.loadOptions();
    }

    // Find the option with the given ID
    const option = this.availableOptions().find(opt => opt.id === id);
    if (option) {
      this.selectedOption.set(option);
      this.searchText.set(option.name);
    }
  }
}
