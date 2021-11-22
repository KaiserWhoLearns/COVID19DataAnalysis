import matplotlib.pyplot as plt
import numpy
import pandas

from data import loader
from helpers import geomap, heatmap
from helpers.compute import roundup

__NUMBER_OF_FILMS_IN_STRIP = 12


def plot_case_and_death_timeline_on_map(state_name='Washington', state_column_filter='Admin1'):
    state_fips_code = geomap.lookup_fips_code(state=state_name)
    state_geo_data = geomap.get_usa_county_level_data(state_fips_code=state_fips_code)
    usa_county_level_cases, usa_county_level_deaths = loader.get_united_states_case_and_death_time_series_data()
    state_cases = usa_county_level_cases[usa_county_level_cases[state_column_filter] == state_name]
    state_deaths = usa_county_level_deaths[usa_county_level_deaths[state_column_filter] == state_name]

    state_case_heatmap = heatmap.process_heatmap_data(state_cases, 'Admin2')
    state_death_heatmap = heatmap.process_heatmap_data(state_deaths, 'Admin2')

    state_case_matrix = []
    state_death_matrix = []

    state_case_matrix_columns = [f'CaseStripMean{d + 1}' for d in range(__NUMBER_OF_FILMS_IN_STRIP)]
    state_death_matrix_columns = [f'DeathStripMean{d + 1}' for d in range(__NUMBER_OF_FILMS_IN_STRIP)]

    state_counties = state_geo_data['NAME'].to_list()
    dataset_counties = set(state_cases['Admin2'].to_list())

    for county in state_counties:
        case_result = [None] * __NUMBER_OF_FILMS_IN_STRIP
        death_result = [None] * __NUMBER_OF_FILMS_IN_STRIP
        if county not in state_case_heatmap:
            print(f"Ignoring {county}")
            state_case_matrix.append(case_result)
            state_death_matrix.append(death_result)
            continue
        cases_count = state_case_heatmap[county]
        death_count = state_death_heatmap[county]

        case_film_strip_chunks = numpy.array_split(cases_count, __NUMBER_OF_FILMS_IN_STRIP)
        death_film_strip_chunks = numpy.array_split(death_count, __NUMBER_OF_FILMS_IN_STRIP)

        for index, chunk_data in enumerate(case_film_strip_chunks):
            case_result[index] = numpy.mean(chunk_data)
        for index, chunk_data in enumerate(death_film_strip_chunks):
            death_result[index] = numpy.mean(chunk_data)

        state_case_matrix.append(case_result)
        state_death_matrix.append(death_result)

    case_matrix_df = pandas.DataFrame(data=state_case_matrix, columns=state_case_matrix_columns)
    death_matrix_df = pandas.DataFrame(data=state_death_matrix, columns=state_death_matrix_columns)

    df = pandas.concat([state_geo_data, case_matrix_df, death_matrix_df], axis=1)

    # Save intermediate data for the state timeline visualization!
    # df.to_csv(f'results/{state_name}.csv', header=True, index=False)

    calibrate_min = 0
    calibrate_case_max = roundup(case_matrix_df.max().max())
    calibrate_death_max = roundup(death_matrix_df.max().max())
    # print(f'Cases: {calibrate_min} --> {calibrate_case_max}')
    # print(f'Death: {calibrate_min} --> {calibrate_death_max}')

    fig, ax = plt.subplots(nrows=2, ncols=__NUMBER_OF_FILMS_IN_STRIP, figsize=(70, 20), sharex=True, sharey=True)
    # Plot all the cases in the first row
    for plot_row_index, (columns, vmax_filter) in enumerate([(state_case_matrix_columns, calibrate_case_max),
                                                             (state_death_matrix_columns, calibrate_death_max)]):
        for column_index, filter_name in enumerate(columns):
            df.plot(
                column=filter_name,
                legend=True,
                ax=ax[plot_row_index][column_index],
                vmin=calibrate_min,
                vmax=vmax_filter,
                cmap='viridis',
                missing_kwds={
                    "color": "lightgrey",
                    "edgecolor": "red",
                    "hatch": "///",
                    "label": "Missing values",
                },
                legend_kwds={
                    'orientation': 'horizontal',
                    'label': 'Cases per {}'.format(heatmap.get_normalized_per_population_value()),
                },
            )
            ax[plot_row_index][column_index].set_axis_off()
    plt.tight_layout()
    plt.savefig(f'results/filmstrip_{state_name}.pdf', bbox_inches='tight')


if __name__ == '__main__':
    states = ['Washington', 'California', 'Oregon', 'Florida', 'New York', 'Arizona', 'North Dakota', 'South Dakota']
    # Examples!
    for state in states:
        print(f"Plotting case and death timeline for {state}")
        plot_case_and_death_timeline_on_map(state_name=state)

