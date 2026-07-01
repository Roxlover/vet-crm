<template>
  <div class="p-4 sm:p-6 lg:p-8 max-w-7xl mx-auto space-y-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Hastalık Kütüphanesi</h1>
        <p class="mt-2 text-sm text-gray-700 dark:text-gray-300">
          Sistemde tanımlı hastalıkların ve teşhis kategorilerinin yönetimi.
        </p>
      </div>
      <div class="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
        <button
          @click="openModal()"
          class="block rounded-md bg-indigo-600 px-3 py-2 text-center text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
        >
          Yeni Hastalık Ekle
        </button>
      </div>
    </div>

    <div class="mt-8 flow-root">
      <div class="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
        <div class="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
          <div class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
            <table class="min-w-full divide-y divide-gray-300 dark:divide-gray-700">
              <thead class="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th scope="col" class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 dark:text-white sm:pl-6">İsim</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">Kategori</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">Tür</th>
                  <th scope="col" class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 dark:text-white">Bulaşıcı Mı?</th>
                  <th scope="col" class="relative py-3.5 pl-3 pr-4 sm:pr-6">
                    <span class="sr-only">Düzenle</span>
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 dark:divide-gray-700 bg-white dark:bg-gray-900">
                <tr v-for="disease in diseases" :key="disease.id">
                  <td class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 dark:text-white sm:pl-6">
                    {{ disease.name }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-400">
                    {{ disease.category }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-400">
                    {{ disease.species || '-' }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500 dark:text-gray-400">
                    <span v-if="disease.isContagious" class="inline-flex items-center rounded-md bg-red-50 px-2 py-1 text-xs font-medium text-red-700 ring-1 ring-inset ring-red-600/10">Evet</span>
                    <span v-else class="inline-flex items-center rounded-md bg-green-50 px-2 py-1 text-xs font-medium text-green-700 ring-1 ring-inset ring-green-600/20">Hayır</span>
                  </td>
                  <td class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                    <button @click="openModal(disease)" class="text-indigo-600 hover:text-indigo-900 dark:text-indigo-400 dark:hover:text-indigo-300 mr-4">
                      Düzenle
                    </button>
                    <button @click="confirmDelete(disease.id)" class="text-red-600 hover:text-red-900 dark:text-red-400 dark:hover:text-red-300">
                      Sil
                    </button>
                  </td>
                </tr>
                <tr v-if="diseases.length === 0">
                  <td colspan="5" class="py-4 text-center text-sm text-gray-500">Gösterilecek kayıt bulunamadı.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Edit Modal -->
    <div v-if="isModalOpen" class="relative z-10" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"></div>

      <div class="fixed inset-0 z-10 w-screen overflow-y-auto">
        <div class="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
          <div class="relative transform overflow-hidden rounded-lg bg-white dark:bg-gray-800 px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
            <div>
              <div class="mt-3 text-center sm:mt-5">
                <h3 class="text-base font-semibold leading-6 text-gray-900 dark:text-white" id="modal-title">
                  {{ isEditing ? 'Hastalık Düzenle' : 'Yeni Hastalık Ekle' }}
                </h3>
                <div class="mt-4 space-y-4 text-left">
                  
                  <div>
                    <label for="name" class="block text-sm font-medium leading-6 text-gray-900 dark:text-gray-300">İsim</label>
                    <div class="mt-2">
                      <input v-model="form.name" type="text" name="name" id="name" class="block w-full rounded-md border-0 py-1.5 text-gray-900 dark:text-white dark:bg-gray-700 shadow-sm ring-1 ring-inset ring-gray-300 dark:ring-gray-600 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6">
                    </div>
                  </div>

                  <div>
                    <label for="category" class="block text-sm font-medium leading-6 text-gray-900 dark:text-gray-300">Kategori</label>
                    <div class="mt-2">
                      <select v-model="form.category" id="category" name="category" class="block w-full rounded-md border-0 py-1.5 text-gray-900 dark:text-white dark:bg-gray-700 shadow-sm ring-1 ring-inset ring-gray-300 dark:ring-gray-600 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6">
                        <option value="Enfeksiyoz">Enfeksiyöz</option>
                        <option value="Paraziter">Paraziter</option>
                        <option value="Kronik">Kronik</option>
                        <option value="Genetik">Genetik</option>
                        <option value="Diger">Diğer</option>
                      </select>
                    </div>
                  </div>

                  <div>
                    <label for="species" class="block text-sm font-medium leading-6 text-gray-900 dark:text-gray-300">Tür (Örn: Kedi, Köpek, Tümü)</label>
                    <div class="mt-2">
                      <input v-model="form.species" type="text" name="species" id="species" class="block w-full rounded-md border-0 py-1.5 text-gray-900 dark:text-white dark:bg-gray-700 shadow-sm ring-1 ring-inset ring-gray-300 dark:ring-gray-600 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6">
                    </div>
                  </div>

                  <div class="relative flex items-start mt-4">
                    <div class="flex h-6 items-center">
                      <input v-model="form.isContagious" id="isContagious" name="isContagious" type="checkbox" class="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-600">
                    </div>
                    <div class="ml-3 text-sm leading-6">
                      <label for="isContagious" class="font-medium text-gray-900 dark:text-gray-300">Bulaşıcı (Zoonotik/Salgın)</label>
                      <p class="text-gray-500">Diğer hayvanlara veya insanlara bulaşma riski var mı?</p>
                    </div>
                  </div>

                  <div>
                    <label for="description" class="block text-sm font-medium leading-6 text-gray-900 dark:text-gray-300">Açıklama / Tedavi Notu</label>
                    <div class="mt-2">
                      <textarea v-model="form.description" id="description" name="description" rows="3" class="block w-full rounded-md border-0 py-1.5 text-gray-900 dark:text-white dark:bg-gray-700 shadow-sm ring-1 ring-inset ring-gray-300 dark:ring-gray-600 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"></textarea>
                    </div>
                  </div>

                </div>
              </div>
            </div>
            <div class="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
              <button @click="save" type="button" class="inline-flex w-full justify-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 sm:col-start-2">
                Kaydet
              </button>
              <button @click="closeModal" type="button" class="mt-3 inline-flex w-full justify-center rounded-md bg-white dark:bg-gray-700 px-3 py-2 text-sm font-semibold text-gray-900 dark:text-gray-300 shadow-sm ring-1 ring-inset ring-gray-300 dark:ring-gray-600 hover:bg-gray-50 dark:hover:bg-gray-600 sm:col-start-1 sm:mt-0">
                İptal
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getDiseases, createDisease, updateDisease, deleteDisease } from '@/api/diseases';
import { useToast } from 'vue-toastification';

const toast = useToast();
const diseases = ref([]);
const isModalOpen = ref(false);
const isEditing = ref(false);

const form = ref({
  id: null,
  name: '',
  category: 'Enfeksiyoz',
  species: '',
  isContagious: false,
  description: ''
});

const loadDiseases = async () => {
  try {
    const res = await getDiseases({ pageSize: 100 });
    diseases.value = res.data.items;
  } catch (error) {
    toast.error('Hastalıklar yüklenirken hata oluştu.');
  }
};

const openModal = (disease = null) => {
  if (disease) {
    isEditing.value = true;
    form.value = { ...disease };
  } else {
    isEditing.value = false;
    form.value = {
      id: null,
      name: '',
      category: 'Enfeksiyoz',
      species: '',
      isContagious: false,
      description: ''
    };
  }
  isModalOpen.value = true;
};

const closeModal = () => {
  isModalOpen.value = false;
};

const save = async () => {
  try {
    if (!form.value.name) {
      toast.warning('Lütfen hastalık adını girin.');
      return;
    }

    if (isEditing.value) {
      await updateDisease(form.value.id, form.value);
      toast.success('Hastalık güncellendi.');
    } else {
      await createDisease(form.value);
      toast.success('Hastalık eklendi.');
    }
    
    closeModal();
    await loadDiseases();
  } catch (error) {
    toast.error('Kaydedilirken hata oluştu.');
  }
};

const confirmDelete = async (id) => {
  if (confirm('Bu hastalığı silmek istediğinize emin misiniz?')) {
    try {
      await deleteDisease(id);
      toast.success('Hastalık silindi.');
      await loadDiseases();
    } catch (error) {
      toast.error('Silinirken hata oluştu.');
    }
  }
};

onMounted(() => {
  loadDiseases();
});
</script>
